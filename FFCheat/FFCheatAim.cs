using UnityEngine;
using System.IO;

/// <summary>
/// Aimbot + AimSilent.
///
/// Aimbot: Smoothly rotates camera toward nearest enemy in FOV circle.
///
/// AimSilent: "Shoot anywhere, always hit" mechanic.
///   - Player aims wherever they want, camera doesn't move
///   - When shot is fired, bullet origin/direction is silently overridden
///     to point at the best target inside FOV circle
///   - Uses a hijacked transform approach: we briefly redirect the 
///     firing transform toward target for exactly one frame, then restore
///   - Result: bullets hit enemy even though crosshair points elsewhere
/// </summary>
public class FFCheatAim : MonoBehaviour
{
    public static FFCheatAim Instance { get; private set; }

    private Camera    _cam;
    private GameObject[] _enemies = new GameObject[0];
    private float    _nextRefresh = 0f;

    // Silent aim state — maintained per-frame
    private GameObject _silentTarget    = null;
    private Vector3    _silentTargetPos = Vector3.zero;
    private bool       _silentActive    = false;

    // Track fire state to detect shot fired (touch/input)
    private Transform  _savedFireTransform    = null;
    private Quaternion _savedFireRotation     = Quaternion.identity;
    private Vector3    _savedFirePosition     = Vector3.zero;
    private bool       _restoringThisFrame    = false;

    void Awake() { Instance = this; }

    void Start()
    {
        _cam = Camera.main;
    }

    void Update()
    {
        var cfg = FFCheatConfig.Instance;
        if (cfg == null) return;
        if (_cam == null) _cam = Camera.main;
        if (_cam == null) return;

        // Refresh enemy list every 0.25s
        if (Time.time >= _nextRefresh)
        {
            _nextRefresh = Time.time + 0.25f;
            _enemies = GameObject.FindGameObjectsWithTag("Player");
        }

        // Find best target in FOV circle for BOTH features
        GameObject target = FindBestTarget(cfg);

        if (cfg.AimBot && target != null)
        {
            DoAimbot(target, cfg);
        }

        if (cfg.AimSilent)
        {
            // Store target for silent aim — used when shot fires
            _silentTarget    = target;
            _silentTargetPos = target != null
                ? GetAimPoint(target, cfg.AimbotTarget)
                : Vector3.zero;
            _silentActive = target != null;
        }
        else
        {
            _silentActive = false;
            _silentTarget = null;
        }

        // Restore fire transform if we redirected it last frame
        if (_restoringThisFrame && _savedFireTransform != null)
        {
            _savedFireTransform.rotation = _savedFireRotation;
            _restoringThisFrame = false;
        }
    }

    // ── Aimbot ────────────────────────────────────────────────────────────────

    private void DoAimbot(GameObject target, FFCheatConfig cfg)
    {
        Vector3 aimPos = GetAimPoint(target, cfg.AimbotTarget);
        Vector3 dir    = (aimPos - _cam.transform.position).normalized;
        if (dir.magnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        _cam.transform.rotation = Quaternion.Slerp(
            _cam.transform.rotation,
            targetRot,
            cfg.AimbotStrength * Time.deltaTime * 120f
        );
    }

    // ── Silent Aim public API ─────────────────────────────────────────────────
    //
    // Call RedirectFire(fireTransform) from weapon fire hook.
    // Redirects transform toward silent target for this frame.
    // Restores original rotation next frame — player sees nothing.

    public void RedirectFire(Transform fireTransform)
    {
        if (!_silentActive || _silentTarget == null || fireTransform == null) return;

        // Save original rotation
        _savedFireTransform = fireTransform;
        _savedFireRotation  = fireTransform.rotation;

        // Point fire transform at silent aim target
        Vector3 dir = (_silentTargetPos - fireTransform.position).normalized;
        if (dir.magnitude > 0.001f)
        {
            fireTransform.rotation   = Quaternion.LookRotation(dir);
            _restoringThisFrame      = true;
        }
    }

    /// <summary>
    /// Returns the direction bullets should travel for silent aim.
    /// Returns Vector3.zero if no target (use original direction).
    /// Call from bullet spawn hook.
    /// </summary>
    public static Vector3 GetSilentAimDirection(Vector3 muzzlePosition)
    {
        if (Instance == null || !Instance._silentActive) return Vector3.zero;
        Vector3 dir = (Instance._silentTargetPos - muzzlePosition).normalized;
        return dir;
    }

    // ── Target selection ──────────────────────────────────────────────────────

    private GameObject FindBestTarget(FFCheatConfig cfg)
    {
        if (_cam == null) return null;

        GameObject best     = null;
        float      bestDist = float.MaxValue;
        float      halfW    = Screen.width  * 0.5f;
        float      halfH    = Screen.height * 0.5f;
        float      fov      = cfg.FovRadius;  // pixels

        foreach (var enemy in _enemies)
        {
            if (enemy == null) continue;
            if (IsLocalPlayer(enemy)) continue;

            // World → screen
            Vector3 screenPos = _cam.WorldToScreenPoint(
                GetAimPoint(enemy, cfg.AimbotTarget));
            if (screenPos.z < 0f) continue;  // behind camera

            // Distance from screen center in pixels
            float dx = screenPos.x - halfW;
            float dy = screenPos.y - halfH;
            float screenDist = Mathf.Sqrt(dx * dx + dy * dy);

            if (screenDist > fov) continue;    // outside FOV circle

            // Pick nearest to center
            if (screenDist < bestDist)
            {
                bestDist = screenDist;
                best     = enemy;
            }
        }

        return best;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Vector3 GetAimPoint(GameObject go, int part)
    {
        Vector3 base_ = go.transform.position;
        switch (part)
        {
            case 0: return base_ + new Vector3(0, 1.75f, 0);   // head
            case 1: return base_ + new Vector3(0, 1.5f,  0);   // neck
            case 2: return base_ + new Vector3(0, 1.0f,  0);   // chest
            default: return base_ + new Vector3(0, 1.75f, 0);
        }
    }

    private static bool IsLocalPlayer(GameObject go)
    {
        return go.tag == "LocalPlayer"
            || go.GetComponent("LocalPlayer") != null;
    }
}

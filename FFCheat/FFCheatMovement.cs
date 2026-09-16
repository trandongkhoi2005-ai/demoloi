using UnityEngine;

/// <summary>
/// Speed hack, FPS unlock, bullet/fire rate speed.
/// </summary>
public class FFCheatMovement : MonoBehaviour
{
    private CharacterController _cc;
    private Rigidbody           _rb;
    private float _origSpeed  = -1f;
    private float _origFPS    = 60f;
    private float _nextSearch = 0f;

    void Start()
    {
        _origFPS = Application.targetFrameRate > 0 ? Application.targetFrameRate : 60f;
    }

    void Update()
    {
        var cfg = FFCheatConfig.Instance;
        if (cfg == null) return;

        // FPS unlock
        int targetFps = cfg.Fps144 ? 144 : (int)_origFPS;
        if (Application.targetFrameRate != targetFps)
            Application.targetFrameRate = targetFps;

        // Find local player components every 2s
        if (Time.time >= _nextSearch)
        {
            _nextSearch = Time.time + 2f;
            FindLocalPlayer();
        }

        if (cfg.SpeedHack) ApplySpeed(cfg.SpeedMultiplier);
        else                RestoreSpeed();

        if (cfg.BulletSpeed) ApplyBulletSpeed(cfg.BulletMultiplier);
        else                  RestoreBulletSpeed();
    }

    private void FindLocalPlayer()
    {
        // Find local player via common tags/components
        var localGos = GameObject.FindGameObjectsWithTag("LocalPlayer");
        if (localGos.Length == 0)
            localGos = GameObject.FindGameObjectsWithTag("Player");

        foreach (var go in localGos)
        {
            // Prefer the one with "Local" in name or a LocalPlayer component
            bool isLocal = go.tag == "LocalPlayer"
                        || go.GetComponent("LocalPlayer") != null;
            if (!isLocal && localGos.Length > 1) continue;

            _cc = go.GetComponent<CharacterController>();
            _rb = go.GetComponent<Rigidbody>();

            if (_cc != null)
            {
                if (_origSpeed < 0) _origSpeed = _cc.radius; // store original
                break;
            }
        }
    }

    private void ApplySpeed(float multiplier)
    {
        if (_cc != null)
        {
            // CharacterController speed is modified via velocity impulse on FixedUpdate
            // Best approach on Unity: scale Move() calls, but we inject via FixedUpdate
            // Direct approach: override characterController velocity
            Vector3 vel = _cc.velocity;
            if (vel.magnitude > 0.1f)
            {
                Vector3 boosted = vel.normalized * (vel.magnitude * multiplier);
                // Apply as position delta — CharacterController.Move
                _cc.Move(boosted * Time.deltaTime - vel * Time.deltaTime);
            }
        }
        else if (_rb != null)
        {
            Vector3 vel = _rb.velocity;
            if (vel.magnitude > 0.1f && vel.magnitude < 50f * multiplier)
                _rb.velocity = vel.normalized * (vel.magnitude * multiplier);
        }
    }

    private void RestoreSpeed() { }

    // Fire rate: scale Time.timeScale briefly when firing
    // Full approach requires hooking weapon fire method — here we use timeScale trick
    private float _origTimeScale = 1f;
    private float _bulletSpeedTimer = 0f;

    private void ApplyBulletSpeed(float mult)
    {
        // Temporarily increase timeScale which speeds up all physics + animation
        // This accelerates bullet simulation and fire animations
        if (Time.timeScale < mult)
        {
            _origTimeScale = 1f;
            Time.timeScale = Mathf.Min(mult, 10f);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
    }

    private void RestoreBulletSpeed()
    {
        if (Time.timeScale != 1f)
        {
            Time.timeScale      = 1f;
            Time.fixedDeltaTime = 0.02f;
        }
    }

    void OnDestroy()
    {
        Time.timeScale      = 1f;
        Time.fixedDeltaTime = 0.02f;
        Application.targetFrameRate = (int)_origFPS;
    }
}

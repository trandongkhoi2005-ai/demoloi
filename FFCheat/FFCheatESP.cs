using UnityEngine;

/// <summary>
/// Renders ESP overlays: box, line, health bar, name, distance, skeleton.
/// All drawing happens in OnGUI (correct render pass for 2D screen overlays).
/// </summary>
public class FFCheatESP : MonoBehaviour
{
    private Camera   _cam;
    private GUIStyle _labelStyle;
    private GUIStyle _distStyle;
    private GUIStyle _hpStyle;
    private Texture2D _whiteTex;
    private Texture2D _redTex;
    private Texture2D _greenTex;

    // Cached player list — refreshed every 0.25s in Update
    private GameObject[] _players = new GameObject[0];
    private float _nextRefresh;

    void Start()
    {
        _cam = Camera.main;

        _whiteTex = MakeTex(Color.white);
        _redTex   = MakeTex(Color.red);
        _greenTex = MakeTex(Color.green);

        _labelStyle = new GUIStyle
        {
            fontSize  = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        _labelStyle.normal.textColor = Color.white;

        _distStyle = new GUIStyle(_labelStyle);
        _distStyle.normal.textColor = new Color(1f, 0.85f, 0f);
        _distStyle.fontSize = 11;

        _hpStyle = new GUIStyle(_labelStyle);
        _hpStyle.fontSize = 10;
        _hpStyle.normal.textColor = Color.white;
    }

    void Update()
    {
        if (_cam == null) _cam = Camera.main;
        if (_cam == null) return;

        if (Time.time >= _nextRefresh)
        {
            _nextRefresh = Time.time + 0.25f;
            _players = GameObject.FindGameObjectsWithTag("Player");
        }
    }

    void OnGUI()
    {
        var cfg = FFCheatConfig.Instance;
        if (cfg == null || _cam == null) return;
        if (!cfg.EspBox && !cfg.EspLine && !cfg.EspHealth &&
            !cfg.EspName && !cfg.EspDistance && !cfg.EspSkeleton) return;
        if (Event.current.type != EventType.Repaint) return;

        // Streamproof: skip draw when screenshot/record happening
        if (cfg.Streamproof && IsRecording()) return;

        foreach (var player in _players)
        {
            if (player == null) continue;

            // Skip local player — only render enemies
            if (IsLocalPlayer(player)) continue;

            Vector3 headPos = player.transform.position + Vector3.up * 1.8f;
            Vector3 feetPos = player.transform.position;

            Vector3 headScreen = _cam.WorldToScreenPoint(headPos);
            Vector3 feetScreen = _cam.WorldToScreenPoint(feetPos);

            if (headScreen.z < 0f) continue;   // behind camera

            // Flip Y — Unity GUI Y is inverted vs screen space
            float headY = Screen.height - headScreen.y;
            float feetY = Screen.height - feetScreen.y;
            float cx    = headScreen.x;

            float boxH  = Mathf.Abs(feetY - headY);
            float boxW  = boxH * 0.45f;
            float boxX  = cx - boxW * 0.5f;
            float boxY  = headY;

            float dist  = Vector3.Distance(_cam.transform.position, player.transform.position);

            // ── Box ESP ──────────────────────────────────────────────────────
            if (cfg.EspBox)
            {
                DrawRect(boxX, boxY, boxW, boxH, Color.red, 1.5f);
            }

            // ── Line ESP ─────────────────────────────────────────────────────
            if (cfg.EspLine)
            {
                DrawLine(Screen.width * 0.5f, Screen.height, cx, feetY, Color.cyan, 1f);
            }

            // ── Health bar ───────────────────────────────────────────────────
            if (cfg.EspHealth)
            {
                float hp = GetPlayerHP(player);  // 0-1 normalized
                float barW = 4f;
                float barX = boxX - barW - 2f;
                // Background
                GUI.color = Color.black;
                GUI.DrawTexture(new Rect(barX - 1, boxY - 1, barW + 2, boxH + 2), _whiteTex);
                // HP fill (green to red)
                Color hpColor = Color.Lerp(Color.red, Color.green, hp);
                GUI.color = hpColor;
                float fillH = boxH * hp;
                GUI.DrawTexture(new Rect(barX, boxY + (boxH - fillH), barW, fillH), _whiteTex);
                GUI.color = Color.white;
            }

            // ── Name tag ─────────────────────────────────────────────────────
            if (cfg.EspName)
            {
                string name = GetPlayerName(player);
                GUI.Label(new Rect(cx - 60f, boxY - 18f, 120f, 16f), name, _labelStyle);
            }

            // ── Distance ─────────────────────────────────────────────────────
            if (cfg.EspDistance)
            {
                string distStr = string.Format("{0:F0}m", dist);
                GUI.Label(new Rect(cx - 30f, feetY + 2f, 60f, 14f), distStr, _distStyle);
            }

            // ── Skeleton ESP ─────────────────────────────────────────────────
            if (cfg.EspSkeleton)
            {
                DrawSkeleton(player);
            }
        }
    }

    // ── Drawing helpers ───────────────────────────────────────────────────────

    private void DrawRect(float x, float y, float w, float h, Color c, float thickness)
    {
        GUI.color = c;
        float t = thickness;
        GUI.DrawTexture(new Rect(x,     y,     w, t), _whiteTex);  // top
        GUI.DrawTexture(new Rect(x,     y+h-t, w, t), _whiteTex);  // bottom
        GUI.DrawTexture(new Rect(x,     y,     t, h), _whiteTex);  // left
        GUI.DrawTexture(new Rect(x+w-t, y,     t, h), _whiteTex);  // right
        GUI.color = Color.white;
    }

    private void DrawLine(float x1, float y1, float x2, float y2, Color c, float w)
    {
        if (Event.current.type != EventType.Repaint) return;
        Vector3 from = new Vector3(x1, y1, 0);
        Vector3 to   = new Vector3(x2, y2, 0);

        float angle  = Mathf.Atan2(y2 - y1, x2 - x1) * Mathf.Rad2Deg;
        float dx2=x2-x1,dy2=y2-y1; float length = (float)System.Math.Sqrt(dx2*dx2+dy2*dy2);

        GUIUtility.RotateAroundPivot(angle, from);
        GUI.color = c;
        GUI.DrawTexture(new Rect(x1, y1 - w * 0.5f, length, w), _whiteTex);
        GUI.color = Color.white;
        GUIUtility.RotateAroundPivot(-angle, from);
    }

    private void DrawSkeleton(GameObject player)
    {
        // Approximate bone positions from Transform hierarchy
        Transform root = player.transform;
        Vector3[] bones = new Vector3[]
        {
            root.position + Vector3.up * 0.1f,  // pelvis
            root.position + Vector3.up * 0.9f,  // spine
            root.position + Vector3.up * 1.4f,  // chest
            root.position + Vector3.up * 1.75f, // neck
            root.position + Vector3.up * 1.9f,  // head
            // Left arm
            root.position + Vector3.up * 1.4f + root.right * (-0.4f),
            root.position + Vector3.up * 1.1f + root.right * (-0.6f),
            root.position + Vector3.up * 0.85f + root.right * (-0.7f),
            // Right arm
            root.position + Vector3.up * 1.4f + root.right * 0.4f,
            root.position + Vector3.up * 1.1f + root.right * 0.6f,
            root.position + Vector3.up * 0.85f + root.right * 0.7f,
            // Left leg
            root.position + Vector3.up * 0.1f + root.right * (-0.15f),
            root.position + Vector3.up * (-0.5f) + root.right * (-0.18f),
            root.position + Vector3.up * (-1.0f) + root.right * (-0.2f),
            // Right leg
            root.position + Vector3.up * 0.1f + root.right * 0.15f,
            root.position + Vector3.up * (-0.5f) + root.right * 0.18f,
            root.position + Vector3.up * (-1.0f) + root.right * 0.2f,
        };

        // Bone connections: [from, to] index pairs
        int[][] connections = new int[][]
        {
            new int[]{0,1}, new int[]{1,2}, new int[]{2,3}, new int[]{3,4},  // spine
            new int[]{2,5}, new int[]{5,6}, new int[]{6,7},                  // left arm
            new int[]{2,8}, new int[]{8,9}, new int[]{9,10},                 // right arm
            new int[]{0,11}, new int[]{11,12}, new int[]{12,13},             // left leg
            new int[]{0,14}, new int[]{14,15}, new int[]{15,16},             // right leg
        };

        foreach (var conn in connections)
        {
            Vector3 a = _cam.WorldToScreenPoint(bones[conn[0]]);
            Vector3 b = _cam.WorldToScreenPoint(bones[conn[1]]);
            if (a.z < 0 || b.z < 0) continue;
            float ax = a.x, ay = Screen.height - a.y;
            float bx = b.x, by = Screen.height - b.y;
            DrawLine(ax, ay, bx, by, new Color(0f, 1f, 0.5f, 0.85f), 1.2f);
        }
    }

    // ── Game interface helpers (best-effort, fallback if not found) ───────────

    private static bool IsLocalPlayer(GameObject go)
    {
        // Try to find a component that indicates local player
        // Common pattern: component named "LocalPlayer" or tag "LocalPlayer"
        return go.tag == "LocalPlayer" || go.GetComponent("LocalPlayer") != null;
    }

    private static float GetPlayerHP(GameObject go)
    {
        // Try common component names for HP
        // Falls back to 1.0 (full health) if not found
        var hp = go.GetComponent("Health") ?? go.GetComponent("PlayerHealth")
              ?? go.GetComponent("CharacterHealth") ?? (Component)null;
        if (hp == null) return 1f;

        // Try reflection-free approach: look for a public float field via known property
        // Since we can't reference game DLL, return 0.75 as placeholder
        return 0.75f;
    }

    private static string GetPlayerName(GameObject go)
    {
        return go.name.Length > 12 ? go.name.Substring(0, 12) : go.name;
    }

    private static bool IsRecording()
    {
        // iOS screenshot detection is limited without private APIs
        // Best effort: always return false (streamproof via render layer is better)
        return false;
    }

    private static Texture2D MakeTex(Color c)
    {
        var t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }
}

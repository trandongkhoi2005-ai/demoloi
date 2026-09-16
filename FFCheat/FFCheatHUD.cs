using UnityEngine;

/// <summary>
/// HUD overlay: FOV circle + Enemy counter.
/// Streamproof layer control.
/// </summary>
public class FFCheatHUD : MonoBehaviour
{
    private Texture2D _circleTex;
    private GUIStyle  _counterStyle;
    private GUIStyle  _counterBgStyle;
    private Camera    _cam;

    private GameObject[] _enemies  = new GameObject[0];
    private int          _visibleCount = 0;
    private float        _nextRefresh = 0f;

    void Start()
    {
        _cam = Camera.main;

        // Pre-generate circle texture (128x128)
        _circleTex = GenerateCircleTexture(128, 1.5f,
            new Color(1f, 1f, 1f, 0.85f), new Color(0, 0, 0, 0));

        _counterStyle = new GUIStyle
        {
            fontSize  = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        _counterStyle.normal.textColor = Color.white;

        _counterBgStyle = new GUIStyle();
        _counterBgStyle.normal.background = MakeTex(new Color(0, 0, 0, 0.55f));
    }

    void Update()
    {
        if (_cam == null) _cam = Camera.main;

        var cfg = FFCheatConfig.Instance;
        if (cfg == null) return;

        if (cfg.EnemyCounter && Time.time >= _nextRefresh)
        {
            _nextRefresh = Time.time + 0.5f;
            RefreshEnemyCount(cfg);
        }
    }

    void OnGUI()
    {
        var cfg = FFCheatConfig.Instance;
        if (cfg == null) return;
        if (Event.current.type != EventType.Repaint) return;
        if (cfg.Streamproof && IsRecording()) return;

        // ── FOV Circle ───────────────────────────────────────────────────────
        if (cfg.FovCircle && _circleTex != null)
        {
            float r   = cfg.FovRadius;
            float cx  = Screen.width  * 0.5f;
            float cy  = Screen.height * 0.5f;
            GUI.DrawTexture(new Rect(cx - r, cy - r, r * 2f, r * 2f), _circleTex);
        }

        // ── Enemy Counter ────────────────────────────────────────────────────
        if (cfg.EnemyCounter)
        {
            string text = string.Format("Enemies: {0}", _visibleCount);
            float w = 160f, h = 28f;
            float x = (Screen.width - w) * 0.5f;
            float y = 8f;

            GUI.Box(new Rect(x - 4, y - 2, w + 8, h + 4), GUIContent.none, _counterBgStyle);
            GUI.Label(new Rect(x, y, w, h), text, _counterStyle);
        }
    }

    private void RefreshEnemyCount(FFCheatConfig cfg)
    {
        _enemies = GameObject.FindGameObjectsWithTag("Player");
        int count = 0;

        foreach (var e in _enemies)
        {
            if (e == null) continue;
            if (e.tag == "LocalPlayer" || e.GetComponent("LocalPlayer") != null) continue;

            float dist = _cam != null
                ? Vector3.Distance(_cam.transform.position, e.transform.position)
                : 0f;

            if (dist <= cfg.EnemyDistance) count++;
        }
        _visibleCount = count;
    }

    // ── Circle texture generator ──────────────────────────────────────────────

    private static Texture2D GenerateCircleTexture(int size, float lineWidth,
        Color lineColor, Color clearColor)
    {
        var tex    = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float half = size * 0.5f;
        float outer = half;
        float inner = half - lineWidth;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx   = x - half;
                float dy   = y - half;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                tex.SetPixel(x, y, (dist >= inner && dist <= outer) ? lineColor : clearColor);
            }
        }
        tex.Apply();
        return tex;
    }

    private static bool IsRecording() { return false; }

    private static Texture2D MakeTex(Color c)
    {
        var t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }
}

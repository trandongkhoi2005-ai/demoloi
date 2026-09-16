using UnityEngine;
using System.IO;

/// <summary>
/// In-game GUI menu — iOS glassmorphism style.
/// 
/// Trigger: 3-finger triple-tap anywhere on screen.
/// Drag: hold title bar to move window.
/// Design: frosted glass, grey translucent background, rounded elements.
/// Writes changes to localConfig.json immediately on toggle.
/// </summary>
public class FFCheatMenu : MonoBehaviour
{
    // ── Menu state ────────────────────────────────────────────────────────────
    private bool _visible      = false;
    private Rect _windowRect   = new Rect(40, 80, 300, 520);
    private bool _isDragging   = false;
    private Vector2 _dragOffset;

    // ── Touch detection ───────────────────────────────────────────────────────
    // 3-finger triple-tap: track last 3 tap timestamps per finger count
    private float _lastThreeFingerTap  = -999f;
    private float _prevThreeFingerTap  = -999f;
    private float _tripleThreshold     = 0.5f;  // max interval between taps
    private bool  _threeFingerDown     = false;

    // ── Scroll ────────────────────────────────────────────────────────────────
    private Vector2 _scrollPos = new Vector2(0,0);

    // ── Textures ──────────────────────────────────────────────────────────────
    private Texture2D _bgTex;       // dark grey translucent bg
    private Texture2D _panelTex;    // slightly lighter panel
    private Texture2D _toggleOnTex; // blue pill
    private Texture2D _toggleOffTex;// dark pill
    private Texture2D _sliderBgTex;
    private Texture2D _sliderFillTex;
    private Texture2D _thumbTex;
    private Texture2D _headerTex;
    private Texture2D _sectionTex;
    private Texture2D _dividerTex;

    // ── Styles ────────────────────────────────────────────────────────────────
    private GUIStyle _titleStyle;
    private GUIStyle _sectionStyle;
    private GUIStyle _labelStyle;
    private GUIStyle _subLabelStyle;
    private GUIStyle _valueStyle;
    private GUIStyle _windowStyle;
    private GUIStyle _scrollStyle;

    // ── Config path ───────────────────────────────────────────────────────────
    private string _configPath;

    // ── Local settings mirror (synced from FFCheatConfig + user edits) ────────
    private bool _aimbot, _aimSilent, _fovCircle;
    private bool _speedHack, _bulletSpeed, _fps144;
    private bool _streamproof;
    private bool _espBox, _espLine, _espHealth, _espName, _espDistance, _espSkeleton;
    private bool _enemyCounter;
    private float _fovRadius = 100f;
    private float _aimbotStrength = 60f;
    private int   _aimbotTarget = 0;       // 0=head 1=neck 2=body
    private float _enemyDistance = 150f;

    private bool _settingsLoaded = false;

    void Start()
    {
        _configPath = Path.Combine(Application.persistentDataPath, "localConfig.json");
        BuildTextures();
        BuildStyles();
        LoadSettingsFromConfig();

        // Center window
        _windowRect.x = (Screen.width  - _windowRect.width)  * 0.5f;
        _windowRect.y = (Screen.height - _windowRect.height)  * 0.5f;
    }

    // ── Touch detection (3-finger triple tap) ─────────────────────────────────

    void Update()
    {
        DetectTripleTap();
        HandleDrag();
    }

    private void DetectTripleTap()
    {
        if (Input.touchCount == 3)
        {
            // All three fingers just began touching
            bool allBegan = true;
            for (int i = 0; i < 3; i++)
                if (Input.GetTouch(i).phase != TouchPhase.Began) { allBegan = false; break; }

            if (allBegan && !_threeFingerDown)
            {
                _threeFingerDown = true;
                float now = Time.time;

                // Check triple tap timing
                if (now - _prevThreeFingerTap < _tripleThreshold &&
                    now - _lastThreeFingerTap  < _tripleThreshold * 2f)
                {
                    ToggleMenu();
                    _lastThreeFingerTap = -999f;
                    _prevThreeFingerTap = -999f;
                }
                else
                {
                    _prevThreeFingerTap = _lastThreeFingerTap;
                    _lastThreeFingerTap = now;
                }
            }
        }
        else
        {
            _threeFingerDown = false;
        }
    }

    private void HandleDrag()
    {
        if (!_visible) return;
        if (Input.touchCount != 1) { _isDragging = false; return; }

        Touch t = Input.GetTouch(0);
        Vector2 pos = new Vector2(t.position.x, Screen.height - t.position.y);

        // Header bar = top 40px of window
        Rect headerBar = new Rect(_windowRect.x, _windowRect.y, _windowRect.width, 44f);

        if (t.phase == TouchPhase.Began && (pos.x>=headerBar.x&&pos.x<=headerBar.x+headerBar.width&&pos.y>=headerBar.y&&pos.y<=headerBar.y+headerBar.height))
        {
            _isDragging  = true;
            _dragOffset  = new Vector2(_windowRect.x - pos.x, _windowRect.y - pos.y);
        }
        else if (t.phase == TouchPhase.Moved && _isDragging)
        {
            _windowRect.x = pos.x + _dragOffset.x;
            _windowRect.y = pos.y + _dragOffset.y;

            // Keep on screen
            _windowRect.x = Mathf.Clamp(_windowRect.x, 0, Screen.width  - _windowRect.width);
            _windowRect.y = Mathf.Clamp(_windowRect.y, 0, Screen.height - _windowRect.height);
        }
        else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
        {
            _isDragging = false;
        }
    }

    private void ToggleMenu()
    {
        _visible = !_visible;
        if (_visible) LoadSettingsFromConfig();
    }

    // ── OnGUI rendering ───────────────────────────────────────────────────────

    void OnGUI()
    {
        if (!_visible) return;
        if (Event.current.type != EventType.Repaint
         && Event.current.type != EventType.MouseDown
         && Event.current.type != EventType.MouseUp
         && Event.current.type != EventType.MouseDrag
         && Event.current.type != EventType.ScrollWheel) return;

        // Dim overlay behind menu
        GUI.color = new Color(0, 0, 0, 0.45f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _bgTex);
        GUI.color = Color.white;

        // Draw window
        _windowRect = GUI.Window(0xFFEE, _windowRect, DrawWindow, GUIContent.none, _windowStyle);
    }

    private void DrawWindow(int id)
    {
        // ── Header ────────────────────────────────────────────────────────────
        GUI.color = new Color(1,1,1,0f);
        GUI.DrawTexture(new Rect(0, 0, _windowRect.width, 44f), _headerTex);
        GUI.color = Color.white;

        GUI.Label(new Rect(0, 0, _windowRect.width, 44f), "FF External", _titleStyle);

        // Close button
        if (GUI.Button(new Rect(_windowRect.width - 38f, 10f, 26f, 26f), "×", _valueStyle))
        {
            _visible = false;
        }

        // ── Scroll content ────────────────────────────────────────────────────
        float scrollY    = 50f;
        float scrollH    = _windowRect.height - scrollY - 8f;
        Rect  scrollRect = new Rect(0, scrollY, _windowRect.width, scrollH);
        Rect  contentRect= new Rect(0, 0, _windowRect.width - 16f, 1100f);

        _scrollPos = GUI.BeginScrollView(scrollRect, _scrollPos, contentRect, false, false);

        float y = 8f;
        float w = contentRect.width;

        // ── AIMING ────────────────────────────────────────────────────────────
        y = DrawSectionHeader("AIMING", y, w);

        bool prevAimbot   = _aimbot;
        bool prevSilent   = _aimSilent;
        bool prevFov      = _fovCircle;

        y = DrawToggle("FOV Circle",   "Aim radius indicator",     ref _fovCircle,    y, w);
        y = DrawSlider("Radius",        ref _fovRadius,   4f, 200f, y, w);
        y = DrawToggle("AimSilent",    "Shoot anywhere, always hit", ref _aimSilent, y, w);
        y = DrawToggle("Aimbot",       "Auto-aim within FOV",      ref _aimbot,       y, w);
        y = DrawSlider("Aim Speed",     ref _aimbotStrength, 1f, 100f, y, w);
        y = DrawTargetPicker(y, w);

        // ── ESP ───────────────────────────────────────────────────────────────
        y = DrawSectionHeader("ESP", y, w);
        y = DrawToggle("Box ESP",      "Bounding box",             ref _espBox,       y, w);
        y = DrawToggle("Line ESP",     "Line to enemy",            ref _espLine,      y, w);
        y = DrawToggle("Health Bar",   "Enemy HP",                 ref _espHealth,    y, w);
        y = DrawToggle("Name Tag",     "Enemy name",               ref _espName,      y, w);
        y = DrawToggle("Distance",     "Show range",               ref _espDistance,  y, w);
        y = DrawToggle("Skeleton",     "Bone overlay",             ref _espSkeleton,  y, w);

        // ── HUD ───────────────────────────────────────────────────────────────
        y = DrawSectionHeader("HUD", y, w);
        y = DrawToggle("Enemy Counter","Live count at top",        ref _enemyCounter, y, w);
        y = DrawSlider("Max Distance", ref _enemyDistance, 10f, 300f, y, w, "m");

        // ── MOVEMENT ──────────────────────────────────────────────────────────
        y = DrawSectionHeader("MOVEMENT / COMBAT", y, w);
        y = DrawToggle("Speed Hack",   "5× move speed",            ref _speedHack,    y, w);
        y = DrawToggle("Bullet Speed", "Fast fire rate",           ref _bulletSpeed,  y, w);
        y = DrawToggle("FPS Unlock",   "144 FPS",                  ref _fps144,       y, w);

        // ── STEALTH ───────────────────────────────────────────────────────────
        y = DrawSectionHeader("STEALTH", y, w);
        y = DrawToggle("Streamproof",  "Hide from recordings",     ref _streamproof,  y, w);

        // ── APPLY button ──────────────────────────────────────────────────────
        y += 10f;
        GUI.color = new Color(0.29f, 0.48f, 1f, 1f);
        if (GUI.Button(new Rect(12f, y, w - 24f, 44f), "Apply", _titleStyle))
        {
            SaveSettings();
            _visible = false;
        }
        GUI.color = Color.white;
        y += 52f;

        GUI.EndScrollView();
    }

    // ── UI component builders ─────────────────────────────────────────────────

    private float DrawSectionHeader(string title, float y, float w)
    {
        GUI.color = new Color(1,1,1,0.06f);
        GUI.DrawTexture(new Rect(0, y, w, 26f), _sectionTex);
        GUI.color = Color.white;
        GUI.Label(new Rect(12f, y + 4f, w, 20f), title, _sectionStyle);
        return y + 32f;
    }

    private float DrawToggle(string title, string sub, ref bool value, float y, float w)
    {
        float rowH = 50f;

        // Row background on hover
        GUI.color = new Color(1,1,1, 0.02f);
        GUI.DrawTexture(new Rect(0, y, w, rowH), _panelTex);
        GUI.color = Color.white;

        // Divider
        GUI.color = new Color(1,1,1,0.06f);
        GUI.DrawTexture(new Rect(12f, y, w - 12f, 0.5f), _dividerTex);
        GUI.color = Color.white;

        // Text
        GUI.Label(new Rect(14f, y + 7f,  w - 70f, 20f), title, _labelStyle);
        GUI.Label(new Rect(14f, y + 25f, w - 70f, 16f), sub,   _subLabelStyle);

        // Toggle pill
        Rect pillRect = new Rect(w - 58f, y + 13f, 44f, 26f);
        DrawPill(pillRect, value);

        // Touch/click toggle
        if (Event.current.type == EventType.MouseDown &&
            (Event.current.mousePosition.x>=pillRect.x&&Event.current.mousePosition.x<=pillRect.x+pillRect.width&&Event.current.mousePosition.y>=pillRect.y&&Event.current.mousePosition.y<=pillRect.y+pillRect.height))
        {
            value = !value;
            Event.current.Use();
        }

        return y + rowH;
    }

    private float DrawSlider(string label, ref float value, float min, float max,
                             float y, float w, string unit = "")
    {
        GUI.Label(new Rect(14f, y + 2f, w * 0.5f, 18f), label, _subLabelStyle);
        string valStr = unit.Length > 0
            ? string.Format("{0:F0}{1}", value, unit)
            : string.Format("{0:F0}", value);
        GUI.Label(new Rect(w - 60f, y + 2f, 50f, 18f), valStr, _valueStyle);

        // Track background
        float trackY = y + 24f;
        float trackX = 14f;
        float trackW = w - 28f;
        float trackH = 4f;

        GUI.color = new Color(1,1,1,0.14f);
        GUI.DrawTexture(new Rect(trackX, trackY, trackW, trackH), _sliderBgTex);

        // Fill
        float t    = (value - min) / (max - min);
        float fillW= trackW * t;
        GUI.color = new Color(0.29f, 0.48f, 1f, 1f);
        GUI.DrawTexture(new Rect(trackX, trackY, fillW, trackH), _sliderFillTex);
        GUI.color = Color.white;

        // Thumb
        float thumbX = trackX + fillW - 8f;
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(thumbX, trackY - 6f, 16f, 16f), _thumbTex);

        // Drag thumb
        Rect dragArea = new Rect(trackX, trackY - 10f, trackW, 24f);
        if ((Event.current.type == EventType.MouseDown ||
             Event.current.type == EventType.MouseDrag)
             && (Event.current.mousePosition.x>=dragArea.x&&Event.current.mousePosition.x<=dragArea.x+dragArea.width&&Event.current.mousePosition.y>=dragArea.y&&Event.current.mousePosition.y<=dragArea.y+dragArea.height))
        {
            float newT = (Event.current.mousePosition.x - trackX) / trackW;
            value = Mathf.Clamp(min + newT * (max - min), min, max);
            Event.current.Use();
        }

        return y + 42f;
    }

    private float DrawTargetPicker(float y, float w)
    {
        GUI.Label(new Rect(14f, y + 4f, 80f, 18f), "Target", _subLabelStyle);

        string[] targets = new string[] { "Head", "Neck", "Body" };
        float btnW = (w - 28f) / 3f;

        for (int i = 0; i < 3; i++)
        {
            Rect btnRect = new Rect(14f + i * btnW, y + 22f, btnW - 4f, 28f);
            bool selected = _aimbotTarget == i;

            GUI.color = selected
                ? new Color(0.29f, 0.48f, 1f, 1f)
                : new Color(1f,1f,1f,0.10f);
            GUI.DrawTexture(btnRect, _panelTex);
            GUI.color = selected ? Color.white : new Color(1,1,1,0.6f);
            GUI.Label(btnRect, targets[i], _labelStyle);
            GUI.color = Color.white;

            if (Event.current.type == EventType.MouseDown &&
                (Event.current.mousePosition.x>=btnRect.x&&Event.current.mousePosition.x<=btnRect.x+btnRect.width&&Event.current.mousePosition.y>=btnRect.y&&Event.current.mousePosition.y<=btnRect.y+btnRect.height))
            {
                _aimbotTarget = i;
                Event.current.Use();
            }
        }

        return y + 58f;
    }

    private void DrawPill(Rect r, bool on)
    {
        GUI.color = on
            ? new Color(0.29f, 0.48f, 1f, 1f)
            : new Color(0.35f, 0.35f, 0.38f, 1f);
        GUI.DrawTexture(r, on ? _toggleOnTex : _toggleOffTex);

        // Thumb
        float thumbX = on ? r.x + r.width - r.height + 3f : r.x + 3f;
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(thumbX, r.y + 3f, r.height - 6f, r.height - 6f), _thumbTex);
        GUI.color = Color.white;
    }

    // ── Texture builders ──────────────────────────────────────────────────────

    private void BuildTextures()
    {
        _bgTex        = MakeTex(new Color(0f, 0f, 0f, 1f));
        _panelTex     = MakeTex(new Color(0.14f, 0.14f, 0.16f, 1f));
        _toggleOnTex  = MakeTex(new Color(0.29f, 0.48f, 1f, 1f));
        _toggleOffTex = MakeTex(new Color(0.28f, 0.28f, 0.30f, 1f));
        _sliderBgTex  = MakeTex(new Color(0.28f, 0.28f, 0.30f, 1f));
        _sliderFillTex= MakeTex(new Color(0.29f, 0.48f, 1f, 1f));
        _thumbTex     = MakeCircleTex(32, Color.white);
        _headerTex    = MakeTex(new Color(0.11f, 0.11f, 0.13f, 1f));
        _sectionTex   = MakeTex(new Color(1f, 1f, 1f, 0.05f));
        _dividerTex   = MakeTex(new Color(1f, 1f, 1f, 0.08f));
    }

    private void BuildStyles()
    {
        _windowStyle = new GUIStyle();
        _windowStyle.normal.background = MakeTex(new Color(0.12f, 0.12f, 0.14f, 0.96f));

        _titleStyle = new GUIStyle
        {
            fontSize  = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        _titleStyle.normal.textColor = Color.white;

        _sectionStyle = new GUIStyle
        {
            fontSize  = 10,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        _sectionStyle.normal.textColor = new Color(1f, 1f, 1f, 0.40f);

        _labelStyle = new GUIStyle
        {
            fontSize  = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        _labelStyle.normal.textColor = Color.white;

        _subLabelStyle = new GUIStyle
        {
            fontSize  = 11,
            alignment = TextAnchor.UpperLeft
        };
        _subLabelStyle.normal.textColor = new Color(1f, 1f, 1f, 0.50f);

        _valueStyle = new GUIStyle
        {
            fontSize  = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleRight
        };
        _valueStyle.normal.textColor = new Color(0.47f, 0.65f, 1f, 1f);

        _scrollStyle = new GUIStyle();
    }

    // ── Config I/O ────────────────────────────────────────────────────────────

    private void LoadSettingsFromConfig()
    {
        var cfg = FFCheatConfig.Instance;
        if (cfg == null) return;

        _aimbot        = cfg.AimBot;
        _aimSilent     = cfg.AimSilent;
        _fovCircle     = cfg.FovCircle;
        _speedHack     = cfg.SpeedHack;
        _bulletSpeed   = cfg.BulletSpeed;
        _fps144        = cfg.Fps144;
        _streamproof   = cfg.Streamproof;
        _espBox        = cfg.EspBox;
        _espLine       = cfg.EspLine;
        _espHealth     = cfg.EspHealth;
        _espName       = cfg.EspName;
        _espDistance   = cfg.EspDistance;
        _espSkeleton   = cfg.EspSkeleton;
        _enemyCounter  = cfg.EnemyCounter;
        _fovRadius     = cfg.FovRadius;
        _aimbotStrength= cfg.AimbotStrength * 1000f;
        _aimbotTarget  = cfg.AimbotTarget;
        _enemyDistance = cfg.EnemyDistance;

        _settingsLoaded = true;
    }

    private void SaveSettings()
    {
        string json = BuildJson();
        try { File.WriteAllText(_configPath, json); }
        catch { }
    }

    private string BuildJson()
    {
        return string.Format(
            "{{\n" +
            "  \"aimbot\": {0},\n" +
            "  \"aimSilent\": {1},\n" +
            "  \"fovCircle\": {2},\n" +
            "  \"speedHack\": {3},\n" +
            "  \"bulletSpeed\": {4},\n" +
            "  \"fps144\": {5},\n" +
            "  \"streamproof\": {6},\n" +
            "  \"espBox\": {7},\n" +
            "  \"espLine\": {8},\n" +
            "  \"espHealth\": {9},\n" +
            "  \"espName\": {10},\n" +
            "  \"espDistance\": {11},\n" +
            "  \"espSkeleton\": {12},\n" +
            "  \"enemyCounter\": {13},\n" +
            "  \"fovRadius\": {14},\n" +
            "  \"aimbotStrength\": {15},\n" +
            "  \"aimbotTarget\": {16},\n" +
            "  \"enemyDistance\": {17},\n" +
            "  \"speedMultiplier\": 5,\n" +
            "  \"bulletMultiplier\": 10\n" +
            "}}",
            B(_aimbot),     B(_aimSilent),    B(_fovCircle),
            B(_speedHack),  B(_bulletSpeed),  B(_fps144),
            B(_streamproof),
            B(_espBox),     B(_espLine),      B(_espHealth),
            B(_espName),    B(_espDistance),  B(_espSkeleton),
            B(_enemyCounter),
            (int)_fovRadius,
            (int)_aimbotStrength,
            _aimbotTarget,
            (int)_enemyDistance
        );
    }

    private static string B(bool v) { return v ? "true" : "false"; }

    // ── Texture helpers ───────────────────────────────────────────────────────

    private static Texture2D MakeTex(Color c)
    {
        var t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }

    private static Texture2D MakeCircleTex(int size, Color c)
    {
        var t    = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float h  = size * 0.5f;
        float r2 = (h - 0.5f) * (h - 0.5f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - h, dy = y - h;
                t.SetPixel(x, y, dx*dx+dy*dy <= r2 ? c : new Color(0,0,0,0));
            }
        }
        t.Apply();
        return t;
    }
}

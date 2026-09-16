using UnityEngine;
using System.IO;
using System.Collections;

/// <summary>
/// Reads feature toggles and slider values from localConfig.json in game Documents folder.
/// All other scripts query FFCheatConfig.Instance for current settings.
/// </summary>
public class FFCheatConfig : MonoBehaviour
{
    public static FFCheatConfig Instance { get; private set; }

    // ── Feature toggles ──────────────────────────────────────────────────────
    public bool AimBot      { get; private set; }
    public bool AimSilent   { get; private set; }
    public bool FovCircle   { get; private set; }
    public bool SpeedHack   { get; private set; }
    public bool BulletSpeed { get; private set; }
    public bool Fps144      { get; private set; }
    public bool Streamproof { get; private set; }
    public bool EspBox      { get; private set; }
    public bool EspLine     { get; private set; }
    public bool EspHealth   { get; private set; }
    public bool EspName     { get; private set; }
    public bool EspDistance { get; private set; }
    public bool EspSkeleton { get; private set; }
    public bool EnemyCounter { get; private set; }

    // ── Slider values ────────────────────────────────────────────────────────
    public float FovRadius       { get; private set; } = 100f;
    public float AimbotStrength  { get; private set; } = 0.15f;  // LookRotation lerp speed
    public int   AimbotTarget    { get; private set; } = 0;      // 0=head 1=neck 2=body
    public float EnemyDistance   { get; private set; } = 150f;
    public float SpeedMultiplier { get; private set; } = 5f;
    public float BulletMultiplier { get; private set; } = 10f;

    private string _configPath;
    private float  _nextRefresh;
    private const float RefreshInterval = 2f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Documents folder path (iOS + Android)
        string docsPath = Application.persistentDataPath;
        _configPath = Path.Combine(docsPath, "localConfig.json");

        LoadConfig();
    }

    void Update()
    {
        if (Time.time >= _nextRefresh)
        {
            _nextRefresh = Time.time + RefreshInterval;
            LoadConfig();
        }
    }

    private void LoadConfig()
    {
        if (!File.Exists(_configPath)) return;

        try
        {
            string json = File.ReadAllText(_configPath);
            ParseJson(json);
        }
        catch { }
    }

    // Minimal JSON parser — no external dependency
    private void ParseJson(string json)
    {
        AimBot       = GetBool(json, "aimbot",       false);
        AimSilent    = GetBool(json, "aimSilent",    false);
        FovCircle    = GetBool(json, "fovCircle",    false);
        SpeedHack    = GetBool(json, "speedHack",    false);
        BulletSpeed  = GetBool(json, "bulletSpeed",  false);
        Fps144       = GetBool(json, "fps144",       false);
        Streamproof  = GetBool(json, "streamproof",  false);
        EspBox       = GetBool(json, "espBox",       false);
        EspLine      = GetBool(json, "espLine",      false);
        EspHealth    = GetBool(json, "espHealth",    false);
        EspName      = GetBool(json, "espName",      false);
        EspDistance  = GetBool(json, "espDistance",  false);
        EspSkeleton  = GetBool(json, "espSkeleton",  false);
        EnemyCounter = GetBool(json, "enemyCounter", false);

        FovRadius        = GetFloat(json, "fovRadius",    100f);
        AimbotStrength   = GetFloat(json, "aimbotStrength", 60f) / 1000f;
        AimbotTarget     = GetInt(json, "aimbotTarget", 0);
        EnemyDistance    = GetFloat(json, "enemyDistance", 150f);
        SpeedMultiplier  = GetFloat(json, "speedMultiplier", 5f);
        BulletMultiplier = GetFloat(json, "bulletMultiplier", 10f);
    }

    // ── Minimal JSON field extractors ─────────────────────────────────────────

    private static bool GetBool(string json, string key, bool def)
    {
        string val = FindValue(json, key);
        if (val == null) return def;
        return val.Trim() == "true";
    }

    private static float GetFloat(string json, string key, float def)
    {
        string val = FindValue(json, key);
        if (val == null) return def;
        float result;
        return float.TryParse(val.Trim(), out result) ? result : def;
    }

    private static int GetInt(string json, string key, int def)
    {
        string val = FindValue(json, key);
        if (val == null) return def;
        int result;
        return int.TryParse(val.Trim(), out result) ? result : def;
    }

    private static string FindValue(string json, string key)
    {
        string searchKey = "\"" + key + "\"";
        int keyIdx = json.IndexOf(searchKey);
        if (keyIdx < 0) return null;

        int colonIdx = json.IndexOf(':', keyIdx + searchKey.Length);
        if (colonIdx < 0) return null;

        int start = colonIdx + 1;
        while (start < json.Length && json[start] == ' ') start++;

        int end = start;
        while (end < json.Length && json[end] != ',' && json[end] != '}' && json[end] != '\n')
            end++;

        return json.Substring(start, end - start).Trim().Trim('"');
    }
}

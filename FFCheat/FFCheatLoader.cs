using UnityEngine;

/// <summary>
/// Entry point — attach all cheat components to a persistent GameObject.
/// Bootstrap() is called by ILFix hook on Camera.Start or early game method.
/// </summary>
public class FFCheatLoader : MonoBehaviour
{
    private static bool _loaded = false;

    public static void Bootstrap()
    {
        if (_loaded) return;
        _loaded = true;

        var go = new GameObject("__FFExternal__");
        DontDestroyOnLoad(go);

        go.AddComponent<FFCheatConfig>();
        go.AddComponent<FFCheatESP>();
        go.AddComponent<FFCheatAim>();
        go.AddComponent<FFCheatMovement>();
        go.AddComponent<FFCheatHUD>();
        go.AddComponent<FFCheatMenu>();   // in-game menu (3-finger triple-tap)

        Debug.Log("[FFExternal] Loaded. 3-finger triple-tap to open menu.");
    }

    void Awake() { Bootstrap(); }
}

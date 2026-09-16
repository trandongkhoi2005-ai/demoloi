import Foundation
import CryptoKit

enum SecurityBind {

    static func hwidHash() -> String {
        let data = Data(DeviceID.hwid.utf8)
        let dig  = SHA256.hash(data: data)
        return dig.prefix(8).map { String(format: "%02x", $0) }.joined()
    }

    // MARK: - config.bin (21 bytes — exact original format)
    //
    // Patch reads exactly 21 bytes sequentially:
    // [0]  0x01     marker (always 1)
    // [1]  __aa     aimbot (0x01 = on)
    // [2]  __ebox   ESP box
    // [3]  __espm   ESP master
    // [4]  0x01     __cage (keep original value)
    // [5]  0x01     __cghp (keep)
    // [6]  0x78     __cgwas = 120 (camera angle, fixed)
    // [7]  0x00     __cgoff (off)
    // [8]  __hot    enemy counter
    // [9]  __espon  ESP on master
    // [10] 0x00     __camid
    // [11] __ehead  ESP head/name
    // [12] __efull  ESP skeleton (value 5 from original)
    // [13] __ehp    ESP health (value 10 from original)
    // [14] __eline  ESP line (value 75 from original)
    // [15] 0x00     __elag
    // [16] 0x01     __moco (keep)
    // [17] __edist  ESP distance (value 10 from original)
    // [18] 0x00     __xray (off)
    // [19] 0x00     __xroff
    // [20] 0x00     __mcwas

    static func generateConfigBin(settings: CheatSettings) -> Data {
        var b = [UInt8](repeating: 0, count: 21)

        // Fixed values from original working config
        b[0]  = 0x01  // marker
        b[4]  = 0x01  // cage
        b[5]  = 0x01  // cghp
        b[6]  = 0x78  // cgwas = 120
        b[16] = 0x01  // moco

        // Aimbot/AimSilent - byte[1]
        b[1]  = (settings.aimbot || settings.aimSilent) ? 0x01 : 0x00

        // ESP box - byte[2]
        b[2]  = settings.espBox ? 0x01 : 0x00

        // ESP master - byte[3] (on if ANY esp active)
        let anyESP = settings.espBox || settings.espLine || settings.espHealth
                  || settings.espName || settings.espDistance || settings.espSkeleton
        b[3]  = anyESP ? 0x01 : 0x00

        // Enemy counter - byte[8]
        b[8]  = settings.enemyCounter ? 0x01 : 0x00

        // ESP on - byte[9]
        b[9]  = anyESP ? 0x01 : 0x00

        // ESP head/name - byte[11]
        b[11] = settings.espName ? 0x01 : 0x00

        // ESP skeleton - byte[12] (value 5 from original)
        b[12] = settings.espSkeleton ? 0x05 : 0x00

        // ESP HP bar - byte[13] (value 10 from original)
        b[13] = settings.espHealth ? 0x0A : 0x00

        // ESP line - byte[14] (value 75 from original)
        b[14] = settings.espLine ? 0x4B : 0x00

        // ESP distance - byte[17] (value 10 from original)
        b[17] = settings.espDistance ? 0x0A : 0x00

        return Data(b)
    }

    // MARK: - localConfig.json
    // Keep simple - patch only needs testCodePatch:true to activate
    // Additional keys for future patch versions

    static func generateLocalConfig(settings: CheatSettings) -> Data? {
        // Format matches FFCheatConfig.cs expected JSON keys
        // FFCheatConfig reads these every 2s from Documents/localConfig.json
        let payload: [String: Any] = [
            "aimbot":         settings.aimbot,
            "aimSilent":      settings.aimSilent,
            "fovCircle":      settings.fovCircle,
            "speedHack":      settings.speedHack,
            "bulletSpeed":    settings.bulletSpeed,
            "fps144":         settings.fps144,
            "streamproof":    settings.streamproof,
            "espBox":         settings.espBox,
            "espLine":        settings.espLine,
            "espHealth":      settings.espHealth,
            "espName":        settings.espName,
            "espDistance":    settings.espDistance,
            "espSkeleton":    settings.espSkeleton,
            "enemyCounter":   settings.enemyCounter,
            "fovRadius":      settings.fovRadius,
            "aimbotStrength": settings.aimbotStrength,
            "aimbotTarget":   settings.aimbotTargetRaw,
            "enemyDistance":  settings.enemyDistance,
            "speedMultiplier": 5,
            "bulletMultiplier": 10,
        ]
        return try? JSONSerialization.data(
            withJSONObject: payload,
            options: [.prettyPrinted, .sortedKeys]
        )
    }
}

using System;
using UnityEngine;
using COW;
using COW.GamePlay;
using IFix;

namespace FFEX
{
    // ─────────────────────────────────────────────────────────────────────────
    // IFix Patches — hooks into real game methods via ILFix bridge
    // Compiled against dummy Assembly-CSharp.dll for type references
    // Actual execution hooks into IL2CPP native via IFix runtime bridge
    // ─────────────────────────────────────────────────────────────────────────

    // ── NO RECOIL ────────────────────────────────────────────────────────────
    // Patches COW.GamePlay.PlayerAttributes.GetScatterRate
    // Returns 0 → no bullet spread

    [IFix.CustomBridge(typeof(PlayerAttributes))]
    interface IPlayerAttrPatch
    {
        [IFix.Patch]
        float GetScatterRate();

        [IFix.Patch]
        void set_EatSpeedScale(float v);

        [IFix.Patch]
        void set_FireIntervalScale(float v);

        [IFix.Patch]
        void set_FireIntervalScaleSkill(float v);

        [IFix.Patch]
        void set_FireIntervalScaleTwo(float v);
    }

    // ── AIMBOT / SILENT AIM ──────────────────────────────────────────────────
    [IFix.CustomBridge(typeof(Player))]
    interface IPlayerPatch
    {
        [IFix.Patch]
        bool IsLocalPlayerOutOfControlNeedUpdataAimRotaion();

        [IFix.Patch]
        void UpdateAimRotation();

        [IFix.Patch]
        bool CanSwitchWeapon();

        [IFix.Patch]
        bool NeedAimAssist();
    }

    // ── FPS UNLOCK ───────────────────────────────────────────────────────────
    [IFix.CustomBridge(typeof(GameSettingData))]
    interface IGameSettingPatch
    {
        [IFix.Patch]
        void SetHighFPSSetting(EHighFPS fps);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Patch implementations in game namespace (partial classes)
// ─────────────────────────────────────────────────────────────────────────────

namespace COW.GamePlay
{
    partial class PlayerAttributes
    {
        [IFix.Patch]
        public float GetScatterRate()
        {
            if (FFEX.Config.NoRecoil) return 0f;
            return IFix.ILFixBridge.Call<PlayerAttributes, float>("GetScatterRate", this);
        }

        [IFix.Patch]
        public void set_EatSpeedScale(float v)
        {
            if (FFEX.Config.SpeedHack)
                v = v * FFEX.Config.SpeedMulti;
            IFix.ILFixBridge.Call<PlayerAttributes, float>("set_EatSpeedScale", this, v);
        }

        [IFix.Patch]
        public void set_FireIntervalScale(float v)
        {
            // Fast swap reduces interval (lower = faster)
            if (FFEX.Config.FastSwap) v = v * 0.1f;
            IFix.ILFixBridge.Call<PlayerAttributes, float>("set_FireIntervalScale", this, v);
        }
    }

    partial class Player
    {
        [IFix.Patch]
        public bool IsLocalPlayerOutOfControlNeedUpdataAimRotaion()
        {
            if (FFEX.Config.AimSilent) return true;
            return IFix.ILFixBridge.Call<Player, bool>(
                "IsLocalPlayerOutOfControlNeedUpdataAimRotaion", this);
        }

        [IFix.Patch]
        public bool CanSwitchWeapon()
        {
            if (FFEX.Config.FastSwap) return true;
            return IFix.ILFixBridge.Call<Player, bool>("CanSwitchWeapon", this);
        }

        [IFix.Patch]
        public bool NeedAimAssist()
        {
            if (FFEX.Config.Aimbot) return true;
            return IFix.ILFixBridge.Call<Player, bool>("NeedAimAssist", this);
        }
    }
}

namespace COW
{
    partial class GameSettingData
    {
        [IFix.Patch]
        public void SetHighFPSSetting(EHighFPS fps)
        {
            if (FFEX.Config.Fps144)
                fps = EHighFPS.HighFPS144;
            IFix.ILFixBridge.Call<GameSettingData, EHighFPS>("SetHighFPSSetting", this, fps);
        }
    }
}

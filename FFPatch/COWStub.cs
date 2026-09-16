// COW Game stub for IFix compilation
// Provides type references needed by FFEX patch code
// Does NOT need to match game implementation - just type signatures

using System;
using UnityEngine;

namespace COW
{
    public enum EHighFPS
    {
        HighFPS30  = 0,
        HighFPS60  = 1,
        HighFPS90  = 2,
        HighFPS120 = 3,
        HighFPS144 = 4,
    }

public partial class Player
    {
        public GameObject gameObject;  // <-- Đảm bảo dòng này tồn tại
        public Transform  transform;   // <-- Và dòng này nữa

        // ... các hàm khác giữ nguyên
    }

    public partial class GameFacade
    {
        public static GameFacade Instance { get; private set; }
        public void Awake() { }
    }

    public partial class GameSettingData
    {
        public void SetHighFPSSetting(EHighFPS fps) { }
    }
}

namespace COW.GamePlay
{
    public partial class PlayerAttributes
    {
        public float GetScatterRate() { return 0f; }
        public float get_EatSpeedScale() { return 1f; }
        public void  set_EatSpeedScale(float v) { }
        public float get_FireIntervalScale() { return 1f; }
        public void  set_FireIntervalScale(float v) { }
        public void  set_FireIntervalScaleSkill(float v) { }
        public void  set_FireIntervalScaleTwo(float v) { }
        public float get_SkillScatterRate() { return 0f; }
        public void  set_SkillScatterRate(float v) { }
        public float get_SkillScatterRateSighting() { return 0f; }
        public void  set_SkillScatterRateSighting(float v) { }
    }

    public partial class Player
    {
        public GameObject gameObject;
        public Transform  transform;

        public bool IsLocalPlayerOutOfControlNeedUpdataAimRotaion() { return false; }
        public void UpdateAimRotation() { }
        public bool CanSwitchWeapon() { return true; }
        public bool NeedAimAssist()   { return false; }
        public bool IsVisible()        { return true; }
        public bool IsFiring()         { return false; }
        public bool get_IsDieing()     { return false; }
        public bool IsInStealth()      { return false; }
        public bool LastInFrustum()    { return true; }
        public void SetAimRotation(UnityEngine.Quaternion rot, bool snap) { }
        public void BindTarget(AttackableEntity target) { }
        public void UpdateAimingTarget() { }
        public int  GetHP()    { return 200; }
        public int  GetMaxHP() { return 200; }

        // Camera helpers
        public UnityEngine.Transform GetRuntimeMainCameraTransform() { return null; }
        public UnityEngine.Transform GetHipTF()  { return null; }
        public UnityEngine.Collider  get_HeadCollider() { return null; }
    }

    public class AttackableEntity
    {
        public GameObject gameObject;
        public Transform  transform;
        public void  set_LockedAimingCollider(UnityEngine.Collider c) { }
        public float GetAttackableRadius() { return 1f; }
        public int   GetAttackableID()     { return 0; }
        public UnityEngine.Vector3 GetAttackableCenterWS() { return Vector3.zero; }
    }

    public class WeaponHandler
    {
        public void SwapWeapon()   { }
        public bool CanSwitchWeapon() { return true; }
    }

    public class CameraControllerManager
    {
        public static CameraControllerManager Instance { get; private set; }
    }
}

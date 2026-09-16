using System;
using System.Collections.Generic;
using UnityEngine;
using COW;
using COW.GamePlay;

namespace FFEX
{
    public class Aimbot : MonoBehaviour
    {
        public static Aimbot Instance;
        public static Vector3 SilentDir = Vector3.zero;

        private Camera _cam;
        private List<Player> _enemies = new List<Player>();
        private float _nextScan;

        void Awake() { Instance = this; }

        void Start() { _cam = Camera.main; }

        void Update()
        {
            if (_cam == null) _cam = Camera.main;

            if (Time.time >= _nextScan)
            {
                _nextScan = Time.time + 0.25f;
                ScanEnemies();
            }

            Player target = FindBest();

            if (Config.Aimbot && target != null)
                DoAimbot(target);

            if (Config.AimSilent && target != null)
                SilentDir = (AimPt(target) - _cam.transform.position).normalized;
            else
                SilentDir = Vector3.zero;
        }

        void ScanEnemies()
        {
            _enemies.Clear();
            var all = UnityEngine.Object.FindObjectsOfType<Player>();
            foreach (var p in all)
                if (p != null) _enemies.Add(p);
        }

        void DoAimbot(Player t)
        {
            Vector3 dir = (AimPt(t) - _cam.transform.position).normalized;
            if (dir == Vector3.zero) return;
            _cam.transform.rotation = Quaternion.Slerp(
                _cam.transform.rotation,
                Quaternion.LookRotation(dir),
                Config.AimbotStrength * Time.deltaTime * 100f
            );
        }

        Player FindBest()
        {
            if (_cam == null) return null;
            Player best = null;
            float bestDist = float.MaxValue;

            foreach (var e in _enemies)
            {
                if (e == null) continue;
                Vector3 sp = _cam.WorldToScreenPoint(AimPt(e));
                if (sp.z < 0f) continue;
                float dx = sp.x - Screen.width  * 0.5f;
                float dy = sp.y - Screen.height * 0.5f;
                float sd = Mathf.Sqrt(dx*dx+dy*dy);
                if (sd > Config.FovRadius) continue;
                if (sd < bestDist) { bestDist=sd; best=e; }
            }
            return best;
        }

        static Vector3 AimPt(Player p)
        {
            Transform t = p.gameObject.transform;
            switch (Config.AimbotTarget)
            {
                case 0: return t.position + Vector3.up * 1.75f;
                case 1: return t.position + Vector3.up * 1.50f;
                default: return t.position + Vector3.up * 1.00f;
            }
        }
    }
}

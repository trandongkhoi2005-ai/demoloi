using System;
using System.IO;
using UnityEngine;

namespace FFEX
{
    public static class Config
    {
        // Feature toggles
        public static bool EspBox       = true;
        public static bool EspLine      = true;
        public static bool EspHealth    = true;
        public static bool EspSkeleton  = false;
        public static bool EspXray      = false;
        public static bool EspName      = true;
        public static bool EspDistance  = true;
        public static bool EnemyCounter = true;
        public static bool Aimbot       = false;
        public static bool AimSilent    = false;
        public static bool NoRecoil     = true;
        public static bool FastSwap     = true;
        public static bool SpeedHack    = false;
        public static bool Fps144       = true;
        public static bool FovCircle    = false;

        // Values
        public static float FovRadius      = 120f;
        public static float AimbotStrength = 0.15f;
        public static int   AimbotTarget   = 0;    // 0=head 1=neck 2=body
        public static float SpeedMulti     = 2.0f;
        public static float MaxDistance    = 200f;

        private static string _path;
        private static float  _lastRead;
        private const  float  ReadInterval = 2f;

        public static void Init()
        {
            _path = Path.Combine(Application.persistentDataPath, "localConfig.json");
            Read();
        }

        public static void Tick()
        {
            if (Time.time - _lastRead < ReadInterval) return;
            _lastRead = Time.time;
            Read();
        }

        private static void Read()
        {
            if (!File.Exists(_path)) return;
            try
            {
                string j = File.ReadAllText(_path);
                EspBox       = B(j, "espBox",       EspBox);
                EspLine      = B(j, "espLine",      EspLine);
                EspHealth    = B(j, "espHealth",    EspHealth);
                EspSkeleton  = B(j, "espSkeleton",  EspSkeleton);
                EspXray      = B(j, "espXray",      EspXray);
                EspName      = B(j, "espName",      EspName);
                EspDistance  = B(j, "espDistance",  EspDistance);
                EnemyCounter = B(j, "enemyCounter", EnemyCounter);
                Aimbot       = B(j, "aimbot",       Aimbot);
                AimSilent    = B(j, "aimSilent",    AimSilent);
                NoRecoil     = B(j, "noRecoil",     NoRecoil);
                FastSwap     = B(j, "fastSwap",     FastSwap);
                SpeedHack    = B(j, "speedHack",    SpeedHack);
                Fps144       = B(j, "fps144",       Fps144);
                FovCircle    = B(j, "fovCircle",    FovCircle);
                FovRadius       = F(j, "fovRadius",      FovRadius);
                AimbotStrength  = F(j, "aimbotStrength", 60f) / 1000f;
                AimbotTarget    = I(j, "aimbotTarget",   AimbotTarget);
                SpeedMulti      = F(j, "speedMultiplier", SpeedMulti);
                MaxDistance     = F(j, "enemyDistance",   MaxDistance);
            }
            catch { }
        }

        private static bool  B(string j, string k, bool d)  { var v=V(j,k); return v==null?d:v.Trim()=="true"; }
        private static float F(string j, string k, float d) { var v=V(j,k); float r; return v!=null&&float.TryParse(v.Trim(),out r)?r:d; }
        private static int   I(string j, string k, int d)   { var v=V(j,k); int   r; return v!=null&&int.TryParse(v.Trim(),out r)?r:d; }

        private static string V(string j, string k)
        {
            int i=j.IndexOf("\""+k+"\""); if(i<0) return null;
            int c=j.IndexOf(':',i);       if(c<0) return null;
            int s=c+1; while(s<j.Length&&j[s]==' ')s++;
            int e=s;   while(e<j.Length&&j[e]!=','&&j[e]!='}'&&j[e]!='\n')e++;
            return j.Substring(s,e-s).Trim().Trim('"');
        }

        public static void Save()
        {
            string j = string.Format(
                "{{\"espBox\":{0},\"espLine\":{1},\"espHealth\":{2},\"espSkeleton\":{3}," +
                "\"espXray\":{4},\"espName\":{5},\"espDistance\":{6},\"enemyCounter\":{7}," +
                "\"aimbot\":{8},\"aimSilent\":{9},\"noRecoil\":{10},\"fastSwap\":{11}," +
                "\"speedHack\":{12},\"fps144\":{13},\"fovCircle\":{14}," +
                "\"fovRadius\":{15},\"aimbotStrength\":{16},\"aimbotTarget\":{17}," +
                "\"speedMultiplier\":{18},\"enemyDistance\":{19}}}",
                Bb(EspBox),Bb(EspLine),Bb(EspHealth),Bb(EspSkeleton),
                Bb(EspXray),Bb(EspName),Bb(EspDistance),Bb(EnemyCounter),
                Bb(Aimbot),Bb(AimSilent),Bb(NoRecoil),Bb(FastSwap),
                Bb(SpeedHack),Bb(Fps144),Bb(FovCircle),
                (int)FovRadius, (int)(AimbotStrength*1000f), AimbotTarget,
                SpeedMulti.ToString("F1"), (int)MaxDistance
            );
            try { File.WriteAllText(_path, j); } catch { }
        }

        private static string Bb(bool v) { return v?"true":"false"; }
    }
}

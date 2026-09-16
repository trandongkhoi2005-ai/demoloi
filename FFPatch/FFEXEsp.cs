using System;
using System.Collections.Generic;
using UnityEngine;
using COW;
using COW.GamePlay;

namespace FFEX
{
    public class Esp : MonoBehaviour
    {
        private Camera _cam;
        private List<Player> _players = new List<Player>();
        private float _nextScan;
        private int   _visCount;

        // Textures
        private Texture2D _white;

        // Styles
        private GUIStyle _nameSt, _distSt, _hpSt, _cntSt;
        private bool _inited;

        void Start()
        {
            _cam = Camera.main;
        }

        void Update()
        {
            if (_cam == null) _cam = Camera.main;

            if (Time.time >= _nextScan)
            {
                _nextScan = Time.time + 0.3f;
                ScanPlayers();
            }
        }

        void ScanPlayers()
        {
            _players.Clear();
            _visCount = 0;

            var facade = GameFacade.Instance;
            if (facade == null) return;

            // Find all players via COW game objects
            var objs = UnityEngine.Object.FindObjectsOfType<Player>();
            foreach (var p in objs)
            {
                if (p == null) continue;
                _players.Add(p);

                if (_cam != null)
                {
                    float dist = Vector3.Distance(_cam.transform.position, p.gameObject.transform.position);
                    if (dist <= Config.MaxDistance) _visCount++;
                }
            }
        }

        void OnGUI()
        {
            if (!_inited) { Init(); _inited = true; }
            if (_cam == null) return;
            if (Event.current.type != EventType.Repaint) return;

            bool anyEsp = Config.EspBox || Config.EspLine || Config.EspHealth ||
                          Config.EspSkeleton || Config.EspXray || Config.EspDistance || Config.EspName;

            // Enemy counter
            if (Config.EnemyCounter)
            {
                string ct = string.Format("◈  {0}  ENEMIES", _visCount);
                float cw=190f, ch=28f;
                float cx=(Screen.width-cw)*0.5f;
                GUI.color = new Color(0,0,0,0.55f);
                GUI.DrawTexture(new Rect(cx-8,5,cw+16,ch+4), _white);
                GUI.color = Color.white;
                GUI.Label(new Rect(cx,7,cw,ch), ct, _cntSt);
            }

            // FOV Circle
            if (Config.FovCircle)
                DrawCircle(Screen.width*0.5f, Screen.height*0.5f, Config.FovRadius,
                           new Color(1,1,1,0.75f), 1.5f, 64);

            if (!anyEsp) return;

            foreach (var player in _players)
            {
                if (player == null) continue;

                Transform tr = player.gameObject.transform;
                Vector3 head = tr.position + Vector3.up * 1.8f;
                Vector3 feet = tr.position;

                Vector3 hs = _cam.WorldToScreenPoint(head);
                Vector3 fs = _cam.WorldToScreenPoint(feet);
                if (hs.z < 0f) continue;

                float hY  = Screen.height - hs.y;
                float fY  = Screen.height - fs.y;
                float cx2 = hs.x;
                float boxH = Mathf.Abs(fY - hY);
                float boxW = boxH * 0.42f;
                float boxX = cx2 - boxW * 0.5f;
                float boxY = hY;
                float dist = Vector3.Distance(_cam.transform.position, tr.position);

                if (dist > Config.MaxDistance) continue;

                // Xray glow
                if (Config.EspXray)
                {
                    GUI.color = new Color(0.2f, 0.7f, 1f, 0.10f);
                    GUI.DrawTexture(new Rect(boxX-5,boxY-5,boxW+10,boxH+10), _white);
                    GUI.color = Color.white;
                }

                // Corner bracket box
                if (Config.EspBox)
                    DrawCorners(boxX, boxY, boxW, boxH, new Color(0.2f,0.8f,1f,0.95f), 1.5f);

                // Line
                if (Config.EspLine)
                    DrawLine(Screen.width*0.5f, Screen.height, cx2, fY, new Color(0.2f,0.8f,1f,0.55f), 1f);

                // Health bar
                if (Config.EspHealth)
                {
                    float hp = 0.75f; // fallback - real HP via GetHP hook
                    try { hp = (float)player.GetHP() / (float)player.GetMaxHP(); } catch { }
                    hp = Mathf.Clamp01(hp);
                    float bw=3f, bx=boxX-bw-3f;
                    GUI.color = new Color(0,0,0,0.55f);
                    GUI.DrawTexture(new Rect(bx-1,boxY-1,bw+2,boxH+2), _white);
                    GUI.color = Color.Lerp(Color.red, new Color(0.1f,0.9f,0.2f), hp);
                    float fh = boxH*hp;
                    GUI.DrawTexture(new Rect(bx, boxY+(boxH-fh), bw, fh), _white);
                    GUI.color = Color.white;
                    GUI.Label(new Rect(bx-18, boxY+boxH*0.5f-7, 20, 14),
                              Mathf.RoundToInt(hp*100)+"%", _hpSt);
                }

                // Name
                if (Config.EspName)
                {
                    string n = player.gameObject.name;
                    if (n.Length > 10) n = n.Substring(0, 10);
                    GUI.Label(new Rect(cx2-50, boxY-17, 100, 15), n, _nameSt);
                }

                // Distance
                if (Config.EspDistance)
                    GUI.Label(new Rect(cx2-25, fY+2, 50, 14),
                              Mathf.RoundToInt(dist)+"m", _distSt);

                // Skeleton
                if (Config.EspSkeleton)
                    DrawSkeleton(tr);
            }
        }

        void DrawCorners(float x,float y,float w,float h,Color c,float t)
        {
            float cs = h*0.22f;
            GUI.color=c;
            GUI.DrawTexture(new Rect(x,     y,    cs,t),_white);
            GUI.DrawTexture(new Rect(x,     y,    t,cs),_white);
            GUI.DrawTexture(new Rect(x+w-cs,y,    cs,t),_white);
            GUI.DrawTexture(new Rect(x+w-t, y,    t,cs),_white);
            GUI.DrawTexture(new Rect(x,     y+h-t,cs,t),_white);
            GUI.DrawTexture(new Rect(x,     y+h-cs,t,cs),_white);
            GUI.DrawTexture(new Rect(x+w-cs,y+h-t,cs,t),_white);
            GUI.DrawTexture(new Rect(x+w-t, y+h-cs,t,cs),_white);
            GUI.color=Color.white;
        }

        void DrawLine(float x1,float y1,float x2,float y2,Color c,float w)
        {
            float dx=x2-x1,dy=y2-y1;
            float len=Mathf.Sqrt(dx*dx+dy*dy);
            float ang=Mathf.Atan2(dy,dx)*Mathf.Rad2Deg;
            Vector3 pv=new Vector3(x1,y1,0);
            GUIUtility.RotateAroundPivot(ang,pv);
            GUI.color=c;
            GUI.DrawTexture(new Rect(x1,y1-w*0.5f,len,w),_white);
            GUI.color=Color.white;
            GUIUtility.RotateAroundPivot(-ang,pv);
        }

        void DrawCircle(float cx,float cy,float r,Color c,float t,int segs)
        {
            float step=360f/segs;
            for(int i=0;i<segs;i++)
            {
                float a1=i*step*Mathf.Deg2Rad, a2=(i+1)*step*Mathf.Deg2Rad;
                DrawLine(cx+Mathf.Cos(a1)*r, cy+Mathf.Sin(a1)*r,
                         cx+Mathf.Cos(a2)*r, cy+Mathf.Sin(a2)*r, c, t);
            }
        }

        void DrawSkeleton(Transform tr)
        {
            Vector3[] b = new Vector3[]
            {
                tr.position+Vector3.up*0.05f,
                tr.position+Vector3.up*0.85f,
                tr.position+Vector3.up*1.35f,
                tr.position+Vector3.up*1.65f,
                tr.position+Vector3.up*1.82f,
                tr.position+Vector3.up*1.35f-tr.right*0.38f,
                tr.position+Vector3.up*1.05f-tr.right*0.60f,
                tr.position+Vector3.up*0.80f-tr.right*0.72f,
                tr.position+Vector3.up*1.35f+tr.right*0.38f,
                tr.position+Vector3.up*1.05f+tr.right*0.60f,
                tr.position+Vector3.up*0.80f+tr.right*0.72f,
                tr.position+Vector3.up*0.05f-tr.right*0.13f,
                tr.position-Vector3.up*0.55f-tr.right*0.16f,
                tr.position-Vector3.up*1.05f-tr.right*0.18f,
                tr.position+Vector3.up*0.05f+tr.right*0.13f,
                tr.position-Vector3.up*0.55f+tr.right*0.16f,
                tr.position-Vector3.up*1.05f+tr.right*0.18f,
            };
            int[][] cn = new int[][]{
                new int[]{0,1},new int[]{1,2},new int[]{2,3},new int[]{3,4},
                new int[]{2,5},new int[]{5,6},new int[]{6,7},
                new int[]{2,8},new int[]{8,9},new int[]{9,10},
                new int[]{0,11},new int[]{11,12},new int[]{12,13},
                new int[]{0,14},new int[]{14,15},new int[]{15,16},
            };
            Color sc = new Color(0.15f,0.95f,0.45f,0.80f);
            foreach(var c2 in cn)
            {
                Vector3 a=_cam.WorldToScreenPoint(b[c2[0]]);
                Vector3 bv=_cam.WorldToScreenPoint(b[c2[1]]);
                if(a.z<0||bv.z<0) continue;
                DrawLine(a.x,Screen.height-a.y,bv.x,Screen.height-bv.y,sc,1.2f);
            }
        }

        void Init()
        {
            _white = new Texture2D(1,1); _white.SetPixel(0,0,Color.white); _white.Apply();
            _nameSt = St(13,FontStyle.Bold,   Color.white,               TextAnchor.MiddleCenter);
            _distSt = St(10,FontStyle.Normal, new Color(1f,0.9f,0.2f),   TextAnchor.MiddleCenter);
            _hpSt   = St(9, FontStyle.Normal, Color.white,               TextAnchor.MiddleCenter);
            _cntSt  = St(15,FontStyle.Bold,   Color.white,               TextAnchor.MiddleCenter);
        }

        static GUIStyle St(int sz,FontStyle fs,Color c,TextAnchor a)
        { var s=new GUIStyle{fontSize=sz,fontStyle=fs,alignment=a}; s.normal.textColor=c; return s; }
    }
}

using System;
using System.IO;
using UnityEngine;

namespace FFEX
{
    public class Menu : MonoBehaviour
    {
        // ── State ──────────────────────────────────────────────────────────
        private bool    _open = false;
        private Rect    _win  = new Rect(20, 60, 320, 560);
        private Vector2 _scroll;
        private bool    _drag;
        private Vector2 _dragOff;

        // ── 3-tap detection (anywhere on screen, single finger) ────────────
        private int   _tapCount;
        private float _lastTap;
        private float _firstTap;
        private const float TAP_WINDOW = 0.6f;

        // ── Textures + styles ──────────────────────────────────────────────
        private Texture2D _bg, _panel, _on, _off, _thumb, _div, _hdr, _btn;
        private GUIStyle  _titleSt, _secSt, _lblSt, _subSt, _valSt, _winSt, _closeSt;
        private bool      _inited;

        void Start()
        {
            _win.x = (Screen.width  - _win.width)  * 0.5f;
            _win.y = (Screen.height - _win.height) * 0.5f;
            Config.Init();
        }

        void Update()
        {
            Config.Tick();
            DetectTap();
            if (_open) DragHeader();
        }

        // ── 3-tap anywhere (single finger) ──────────────────────────────────
        void DetectTap()
        {
            if (Input.touchCount != 1) return;
            Touch t = Input.GetTouch(0);
            if (t.phase != TouchPhase.Began) return;

            float now = Time.time;
            if (_tapCount == 0)
            {
                _firstTap = now;
                _tapCount = 1;
            }
            else if (now - _lastTap < TAP_WINDOW)
            {
                _tapCount++;
                if (_tapCount >= 3)
                {
                    _open = !_open;
                    if (_open) Config.Init();
                    _tapCount = 0;
                }
            }
            else
            {
                _tapCount = 1;
                _firstTap = now;
            }
            _lastTap = now;

            // Reset if too slow
            if (now - _firstTap > TAP_WINDOW * 2f) _tapCount = 0;
        }

        void DragHeader()
        {
            if (Input.touchCount != 1) { _drag = false; return; }
            Touch t = Input.GetTouch(0);
            Vector2 p = new Vector2(t.position.x, Screen.height - t.position.y);
            Rect hdr = new Rect(_win.x, _win.y, _win.width, 46f);

            if (t.phase == TouchPhase.Began)
            {
                if (p.x>=hdr.x&&p.x<=hdr.x+hdr.width&&p.y>=hdr.y&&p.y<=hdr.y+hdr.height)
                { _drag=true; _dragOff=new Vector2(_win.x-p.x,_win.y-p.y); }
            }
            else if (t.phase == TouchPhase.Moved && _drag)
            {
                _win.x = Mathf.Clamp(p.x+_dragOff.x, 0, Screen.width -_win.width);
                _win.y = Mathf.Clamp(p.y+_dragOff.y, 0, Screen.height-_win.height);
            }
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                _drag = false;
        }

        void OnGUI()
        {
            if (!_inited) { Build(); _inited=true; }
            if (!_open) return;

            GUI.color = new Color(0,0,0,0.45f);
            GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height), _bg);
            GUI.color = Color.white;

            _win = GUI.Window(0xFFEE, _win, Draw, GUIContent.none, _winSt);
        }

        void Draw(int id)
        {
            float W = _win.width;

            // Header
            GUI.color = new Color(1,1,1,0.07f);
            GUI.DrawTexture(new Rect(0,0,W,46f), _hdr);
            GUI.color = Color.white;
            GUI.Label(new Rect(0,0,W,46f), "FFEX", _titleSt);
            if (GUI.Button(new Rect(W-40f,10f,28f,28f), "×", _closeSt))
                _open = false;

            Rect sRect = new Rect(0, 50f, W, _win.height-58f);
            Rect cRect = new Rect(0, 0,  W-14f, 1300f);
            _scroll = GUI.BeginScrollView(sRect, _scroll, cRect, false, false);

            float y = 6f;

            // ── AIMBOT ──────────────────────────────────────────────────────
            y = Sec("◉  AIMBOT", y, W);
            y = Tog("Aimbot",      "Auto-aim inside FOV",       ref Config.Aimbot,      y, W);
            y = Sld("FOV Radius",  ref Config.FovRadius,  10f,360f, y, W, "px");
            y = Tog("AimSilent",   "Shoot anywhere, always hit",ref Config.AimSilent,   y, W);
            y = Tog("No Recoil",   "Bullet always straight",    ref Config.NoRecoil,    y, W);
            y = Tog("Fast Swap",   "No weapon swap delay",      ref Config.FastSwap,    y, W);
            y = TgtPicker(y, W);
            y = Sld("Aim Speed",   ref Config.AimbotStrength, 0f,1f, y, W, "");
            y = Tog("FOV Circle",  "Aim radius indicator",      ref Config.FovCircle,   y, W);

            // ── ESP ─────────────────────────────────────────────────────────
            y = Sec("◈  ESP", y, W);
            y = Tog("ESP Box",     "Corner brackets",           ref Config.EspBox,      y, W);
            y = Tog("ESP Line",    "Line to enemy",             ref Config.EspLine,     y, W);
            y = Tog("ESP Health",  "Health bar",                ref Config.EspHealth,   y, W);
            y = Tog("ESP Xray",    "See through walls",         ref Config.EspXray,     y, W);
            y = Tog("ESP Skeleton","Bone overlay",              ref Config.EspSkeleton, y, W);
            y = Tog("Enemy Count", "Live count at top",         ref Config.EnemyCounter,y, W);
            y = Sld("Max Dist",    ref Config.MaxDistance, 10f,400f, y, W, "m");

            // ── OTHERS ──────────────────────────────────────────────────────
            y = Sec("◎  OTHERS", y, W);
            y = Tog("Speed Hack",  "Move faster",               ref Config.SpeedHack,  y, W);
            y = Sld("Speed ×",     ref Config.SpeedMulti,  1.2f,5f, y, W, "×");
            y = Tog("FPS 144",     "Unlock frame rate",         ref Config.Fps144,     y, W);

            // Apply
            y += 10f;
            GUI.color = new Color(0.28f,0.47f,1f,0.92f);
            GUI.DrawTexture(new Rect(12f,y,W-24f,42f), _btn);
            GUI.color = Color.white;
            _titleSt.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(12f,y,W-24f,42f), "▶  Apply", _titleSt);
            _titleSt.alignment = TextAnchor.MiddleCenter;

            Rect applyR = new Rect(12f,y,W-24f,42f);
            if (Event.current.type==EventType.MouseDown &&
                applyR.Contains(Event.current.mousePosition))
            {
                Config.Save();
                _open = false;
                Event.current.Use();
            }
            y += 52f;

            GUI.EndScrollView();
        }

        // ── Widget helpers ─────────────────────────────────────────────────

        float Sec(string t, float y, float W)
        {
            GUI.color = new Color(1,1,1,0.06f);
            GUI.DrawTexture(new Rect(0,y,W,26f), _panel);
            GUI.color = new Color(1,1,1,0.38f);
            GUI.Label(new Rect(12f,y+4f,W,18f), t, _secSt);
            GUI.color = Color.white;
            return y+31f;
        }

        float Tog(string title, string sub, ref bool val, float y, float W)
        {
            float rH = 52f;
            GUI.color = new Color(1,1,1,0.02f);
            GUI.DrawTexture(new Rect(0,y,W,rH), _panel);
            GUI.color = new Color(1,1,1,0.055f);
            GUI.DrawTexture(new Rect(12f,y+rH-0.5f,W-12f,0.5f), _div);
            GUI.color = Color.white;
            GUI.Label(new Rect(14f,y+7f, W-70f,20f), title, _lblSt);
            GUI.Label(new Rect(14f,y+27f,W-70f,16f), sub,   _subSt);
            Rect pill = new Rect(W-60f,y+13f,46f,26f);
            DrawPill(pill,val);
            if (Event.current.type==EventType.MouseDown && pill.Contains(Event.current.mousePosition))
            { val=!val; Event.current.Use(); }
            return y+rH;
        }

        float Sld(string lbl, ref float val, float mn, float mx, float y, float W, string u)
        {
            GUI.Label(new Rect(14f,y+2f,120f,18f), lbl, _subSt);
            string vs = u.Length>0 ? string.Format("{0:F1}{1}",val,u) : string.Format("{0:F0}",val);
            GUI.Label(new Rect(W-72f,y+2f,60f,18f), vs, _valSt);
            float tx=14f,ty=y+24f,tw=W-28f,th=4f;
            GUI.color=new Color(1,1,1,0.12f);
            GUI.DrawTexture(new Rect(tx,ty,tw,th),_div);
            float t2=(val-mn)/(mx-mn);
            GUI.color=new Color(0.28f,0.47f,1f,0.9f);
            GUI.DrawTexture(new Rect(tx,ty,tw*t2,th),_div);
            GUI.color=Color.white;
            GUI.DrawTexture(new Rect(tx+tw*t2-8f,ty-6f,16f,16f),_thumb);
            Rect dr=new Rect(tx,ty-10f,tw,24f);
            if((Event.current.type==EventType.MouseDown||Event.current.type==EventType.MouseDrag)
                && dr.Contains(Event.current.mousePosition))
            { val=Mathf.Clamp(mn+(Event.current.mousePosition.x-tx)/tw*(mx-mn),mn,mx); Event.current.Use(); }
            return y+44f;
        }

        float TgtPicker(float y, float W)
        {
            GUI.Label(new Rect(14f,y+4f,80f,18f), "Target", _subSt);
            string[] lbs = new string[]{"Head","Neck","Body"};
            float bw=(W-28f)/3f;
            for(int i=0;i<3;i++)
            {
                bool sel = Config.AimbotTarget==i;
                Rect r = new Rect(14f+i*bw,y+22f,bw-4f,28f);
                GUI.color = sel?new Color(0.28f,0.47f,1f,0.95f):new Color(1f,1f,1f,0.09f);
                GUI.DrawTexture(r,_panel);
                GUI.color = sel?Color.white:new Color(1,1,1,0.55f);
                _lblSt.alignment=TextAnchor.MiddleCenter;
                GUI.Label(r,lbs[i],_lblSt);
                _lblSt.alignment=TextAnchor.MiddleLeft;
                GUI.color=Color.white;
                if(Event.current.type==EventType.MouseDown&&r.Contains(Event.current.mousePosition))
                { Config.AimbotTarget=i; Event.current.Use(); }
            }
            return y+58f;
        }

        void DrawPill(Rect r, bool on)
        {
            GUI.color = on?new Color(0.28f,0.47f,1f,0.95f):new Color(0.30f,0.30f,0.32f,0.90f);
            GUI.DrawTexture(r, on?_on:_off);
            float tx = on?r.x+r.width-r.height+3f:r.x+3f;
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(tx,r.y+3f,r.height-6f,r.height-6f),_thumb);
            GUI.color = Color.white;
        }

        // ── Build textures + styles ────────────────────────────────────────
        void Build()
        {
            _bg    = Mk(new Color(0,0,0,1f));
            _panel = Mk(new Color(0.13f,0.13f,0.15f,1f));
            _on    = Mk(new Color(0.28f,0.47f,1f,1f));
            _off   = Mk(new Color(0.28f,0.28f,0.30f,1f));
            _thumb = Circ(32,Color.white);
            _div   = Mk(new Color(1f,1f,1f,0.08f));
            _hdr   = Mk(new Color(0.10f,0.10f,0.12f,1f));
            _btn   = Mk(Color.white);

            _winSt  = new GUIStyle();
            _winSt.normal.background = Mk(new Color(0.11f,0.11f,0.13f,0.97f));

            _titleSt = St(17,FontStyle.Bold,  Color.white,                TextAnchor.MiddleCenter);
            _secSt   = St(10,FontStyle.Bold,  new Color(1,1,1,0.38f),     TextAnchor.UpperLeft);
            _lblSt   = St(14,FontStyle.Bold,  Color.white,                TextAnchor.MiddleLeft);
            _subSt   = St(11,FontStyle.Normal,new Color(1,1,1,0.48f),     TextAnchor.UpperLeft);
            _valSt   = St(12,FontStyle.Bold,  new Color(0.47f,0.66f,1f),  TextAnchor.MiddleRight);
            _closeSt = St(18,FontStyle.Bold,  new Color(1,1,1,0.7f),      TextAnchor.MiddleCenter);
        }

        static GUIStyle St(int sz,FontStyle fs,Color c,TextAnchor a)
        { var s=new GUIStyle{fontSize=sz,fontStyle=fs,alignment=a}; s.normal.textColor=c; return s; }

        static Texture2D Mk(Color c)
        { var t=new Texture2D(1,1); t.SetPixel(0,0,c); t.Apply(); return t; }

        static Texture2D Circ(int sz,Color c)
        {
            var t=new Texture2D(sz,sz,TextureFormat.RGBA32,false);
            float h=sz*0.5f,r2=(h-0.5f)*(h-0.5f);
            for(int y=0;y<sz;y++) for(int x=0;x<sz;x++)
            { float dx=x-h,dy=y-h; t.SetPixel(x,y,dx*dx+dy*dy<=r2?c:new Color(0,0,0,0)); }
            t.Apply(); return t;
        }
    }
}

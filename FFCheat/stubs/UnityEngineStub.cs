namespace UnityEngine {
    public class Object {
        public string name;
        public static void Destroy(Object o) {}
        public static void DontDestroyOnLoad(Object o) {}
    }
    public class Component : Object {
        public GameObject gameObject;
        public Transform transform;
        public T GetComponent<T>() where T : Component { return null; }
        public object GetComponent(string t) { return null; }
    }
    public class Behaviour : Component { public bool enabled; }
    public class MonoBehaviour : Behaviour {
        public void StartCoroutine(System.Collections.IEnumerator r) {}
    }
    public class GameObject : Object {
        public Transform transform;
        public string tag;
        public GameObject(string n) { name = n; }
        public static GameObject[] FindGameObjectsWithTag(string t) { return new GameObject[0]; }
        public T AddComponent<T>() where T : Component { return null; }
        public T GetComponent<T>() where T : Component { return null; }
        public object GetComponent(string t) { return null; }
    }
    public class Transform : Component {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 right;
        public Vector3 forward;
        public Vector3 up;
    }
    public struct Vector3 {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x=x; this.y=y; this.z=z; }
        public static Vector3 zero = new Vector3(0,0,0);
        public static Vector3 up   = new Vector3(0,1,0);
        public float magnitude { get { return (float)System.Math.Sqrt(x*x+y*y+z*z); } }
        public Vector3 normalized { get { float m=magnitude; return m>0?new Vector3(x/m,y/m,z/m):zero; } }
        public static float Distance(Vector3 a, Vector3 b) { return (a-b).magnitude; }
        public static Vector3 operator+(Vector3 a, Vector3 b) { return new Vector3(a.x+b.x,a.y+b.y,a.z+b.z); }
        public static Vector3 operator-(Vector3 a, Vector3 b) { return new Vector3(a.x-b.x,a.y-b.y,a.z-b.z); }
        public static Vector3 operator*(Vector3 a, float f) { return new Vector3(a.x*f,a.y*f,a.z*f); }
        public static bool operator==(Vector3 a, Vector3 b){return a.x==b.x&&a.y==b.y&&a.z==b.z;}
        public static bool operator!=(Vector3 a, Vector3 b){return !(a==b);}
        public override bool Equals(object o){return o is Vector3 v&&this==v;}
        public override int GetHashCode(){return x.GetHashCode()^y.GetHashCode()^z.GetHashCode();}
    }
    public struct Vector2 {
        public float x, y;
        public static Vector2 zero = new Vector2(0,0);
        public Vector2(float x, float y){this.x=x;this.y=y;}
        public float magnitude { get { return (float)System.Math.Sqrt(x*x+y*y); } }
        public static float Distance(Vector2 a, Vector2 b){float dx=a.x-b.x,dy=a.y-b.y;return (float)System.Math.Sqrt(dx*dx+dy*dy);}
        public static Vector2 operator-(Vector2 a, Vector2 b){return new Vector2(a.x-b.x,a.y-b.y);}
    }
    public struct Quaternion {
        public float x,y,z,w;
        public static Quaternion identity = new Quaternion();
        public static Quaternion LookRotation(Vector3 d) { return identity; }
        public static Quaternion Slerp(Quaternion a, Quaternion b, float t) { return a; }
    }
    public struct Rect {
        public float x,y,width,height;
        public Rect(float x,float y,float w,float h){this.x=x;this.y=y;width=w;height=h;}
        public bool Contains(Vector2 p){return p.x>=x&&p.x<=x+width&&p.y>=y&&p.y<=y+height;}
    }
    public struct Color {
        public float r,g,b,a;
        public Color(float r,float g,float b,float a=1){this.r=r;this.g=g;this.b=b;this.a=a;}
        public static Color red    = new Color(1,0,0);
        public static Color green  = new Color(0,1,0);
        public static Color white  = new Color(1,1,1);
        public static Color black  = new Color(0,0,0);
        public static Color cyan   = new Color(0,1,1);
        public static Color yellow = new Color(1,1,0);
        public static Color Lerp(Color a, Color b, float t) {
            return new Color(a.r+(b.r-a.r)*t,a.g+(b.g-a.g)*t,a.b+(b.b-a.b)*t);
        }
    }
    public class Camera : Behaviour {
        public static Camera main;
        public new Transform transform;
        public Vector3 WorldToScreenPoint(Vector3 p) { return Vector3.zero; }
    }
    public class Texture2D : Object {
        public Texture2D(int w, int h) {}
        public Texture2D(int w, int h, TextureFormat f, bool m) {}
        public void SetPixel(int x, int y, Color c) {}
        public void Apply() {}
    }
    public enum TextureFormat { RGBA32 }
    public enum FontStyle { Normal, Bold, Italic, BoldAndItalic }
    public enum TextAnchor { UpperLeft,UpperCenter,UpperRight,MiddleLeft,MiddleCenter,MiddleRight,LowerLeft,LowerCenter,LowerRight }
    public enum TouchPhase { Began, Moved, Stationary, Ended, Canceled }
    public struct Touch { public int fingerId; public Vector2 position; public TouchPhase phase; }
    public class Input {
        public static int touchCount { get { return 0; } }
        public static Touch GetTouch(int i) { return new Touch(); }
    }
    public class GUIStyle {
        public int fontSize;
        public FontStyle fontStyle;
        public TextAnchor alignment;
        public GUIStyleState normal = new GUIStyleState();
        public GUIStyle() {}
        public GUIStyle(GUIStyle s) {}
    }
    public class GUIStyleState { public Color textColor; public Texture2D background; }
    public class GUIContent {
        public static GUIContent none = new GUIContent();
        public GUIContent() {}
        public GUIContent(string t) {}
    }
    public class Event {
        public static Event current = new Event();
        public EventType type;
        public Vector2 mousePosition;
        public void Use() {}
    }
    public enum EventType { Repaint,Layout,MouseDown,MouseUp,MouseDrag,ScrollWheel,KeyDown,KeyUp }
    public class GUI {
        public delegate void WindowFunction(int id);
        public static Color color;
        public static Rect Window(int id, Rect r, WindowFunction func, GUIContent c, GUIStyle st) { return r; }
        public static void DrawTexture(Rect r, Texture2D t) {}
        public static void Label(Rect r, string s, GUIStyle st) {}
        public static bool Button(Rect r, string s, GUIStyle st) { return false; }
        public static void Box(Rect r, GUIContent c, GUIStyle st) {}
        public static Vector2 BeginScrollView(Rect r, Vector2 pos, Rect content, bool h, bool v) { return pos; }
        public static void EndScrollView() {}
    }
    public class GUIUtility { public static void RotateAroundPivot(float a, Vector3 p) {} }
    public class Time {
        public static float time;
        public static float deltaTime;
        public static float timeScale;
        public static float fixedDeltaTime;
    }
    public class Screen { public static int width; public static int height; }
    public class Application {
        public static int targetFrameRate;
        public static string persistentDataPath = "";
    }
    public class Mathf {
        public static float Sqrt(float f) { return (float)System.Math.Sqrt(f); }
        public static float Clamp(float v, float min, float max) { return v<min?min:v>max?max:v; }
        public static float Atan2(float y, float x) { return (float)System.Math.Atan2(y,x); }
        public static float Min(float a, float b) { return a<b?a:b; }
        public static float Abs(float f) { return f<0?-f:f; }
        public const float Rad2Deg = 57.29578f;
        public const float PI = 3.14159265f;
    }
    public class Debug { public static void Log(object o) {} }
    public class CharacterController : Component {
        public Vector3 velocity;
        public float radius;
        public void Move(Vector3 m) {}
        public void SimpleMove(Vector3 s) {}
    }
    public class Rigidbody : Component { public Vector3 velocity; }
    public struct RaycastHit { public Vector3 point; public Transform transform; }
    public class Physics {
        public static bool Raycast(Vector3 o, Vector3 d, out RaycastHit h, float dist) {
            h = new RaycastHit(); return false;
        }
    }
}

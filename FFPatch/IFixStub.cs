// IFix API stubs - allows compilation without full IFix SDK
// IFix runtime (in game) provides actual implementation

using System;

namespace IFix
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PatchAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Interface)]
    public class CustomBridgeAttribute : Attribute
    {
        public CustomBridgeAttribute(Type type) { }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class InterpretAttribute : Attribute { }

    public static class ILFixBridge
    {
        public static void Call<T>(string method, T instance) { }
        public static void Call<T, A>(string method, T instance, A arg) { }
        public static R Call<T, R>(string method, T instance) { return default(R); }
        public static R Call<T, R, A>(string method, T instance, A arg) { return default(R); }
    }
}

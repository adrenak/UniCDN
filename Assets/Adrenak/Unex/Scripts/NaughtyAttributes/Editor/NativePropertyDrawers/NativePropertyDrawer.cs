using System.Reflection;

namespace Adrenak.Unex.Editor
{
    public abstract class NativePropertyDrawer
    {
        public abstract void DrawNativeProperty(UnityEngine.Object target, PropertyInfo property);
    }
}

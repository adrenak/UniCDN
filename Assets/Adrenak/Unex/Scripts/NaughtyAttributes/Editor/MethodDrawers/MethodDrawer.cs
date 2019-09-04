using System.Reflection;

namespace Adrenak.Unex.Editor
{
    public abstract class MethodDrawer
    {
        public abstract void DrawMethod(UnityEngine.Object target, MethodInfo methodInfo);
    }
}

using System.Reflection;

namespace Adrenak.Unex.Editor
{
    public abstract class FieldDrawer
    {
        public abstract void DrawField(UnityEngine.Object target, FieldInfo field);
    }
}

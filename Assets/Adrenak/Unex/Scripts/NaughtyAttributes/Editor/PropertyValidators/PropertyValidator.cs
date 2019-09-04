using UnityEditor;

namespace Adrenak.Unex.Editor
{
    public abstract class PropertyValidator
    {
        public abstract void ValidateProperty(SerializedProperty property);
    }
}

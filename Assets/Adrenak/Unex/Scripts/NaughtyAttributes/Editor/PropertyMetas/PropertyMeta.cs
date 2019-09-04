using UnityEditor;

namespace Adrenak.Unex.Editor
{
    public abstract class PropertyMeta
    {
        public abstract void ApplyPropertyMeta(SerializedProperty property, MetaAttribute metaAttribute);
    }
}

using UnityEditor;

namespace Adrenak.Unex.Editor
{
    public abstract class PropertyDrawCondition
    {
        public abstract bool CanDrawProperty(SerializedProperty property);
    }
}

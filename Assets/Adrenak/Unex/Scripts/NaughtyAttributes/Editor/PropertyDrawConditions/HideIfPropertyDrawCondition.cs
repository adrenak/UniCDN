using UnityEditor;

namespace Adrenak.Unex.Editor
{
    [PropertyDrawCondition(typeof(HideIfAttribute))]
    public class HideIfPropertyDrawCondition : ShowIfPropertyDrawCondition
    {
    }
}

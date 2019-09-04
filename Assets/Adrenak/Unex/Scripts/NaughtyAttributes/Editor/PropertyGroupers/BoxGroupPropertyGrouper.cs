using UnityEditor;
using UnityEngine;

namespace Adrenak.Unex.Editor
{
    [PropertyGrouper(typeof(BoxGroupAttribute))]
    public class BoxGroupPropertyGrouper : PropertyGrouper
    {
        public override void BeginGroup(string label)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            if (!string.IsNullOrEmpty(label))
            {
                EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            }
        }

        public override void EndGroup()
        {
            EditorGUILayout.EndVertical();
        }
    }
}

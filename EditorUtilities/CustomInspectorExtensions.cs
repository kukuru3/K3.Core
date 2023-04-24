#if UNITY_EDITOR

namespace K3.Editors {

    using UnityEditor;
    using UnityEngine;

    static public class EditorExtensions {
        static public void DoMinMaxField(this Editor editor, string name, SerializedProperty propStart, SerializedProperty propEnd, float limitMin, float limitMax) {
            EditorGUILayout.BeginHorizontal();            
            var valueMin = propStart.floatValue;
            var valueMax = propEnd.floatValue;
            EditorGUILayout.MinMaxSlider(name, ref valueMin, ref valueMax, limitMin, limitMax);
            EditorGUILayout.LabelField($"{valueMin:F0}-{valueMax:F0}", GUILayout.MaxWidth(100));
            propStart.floatValue = valueMin;
            propEnd.floatValue = valueMax;
            EditorGUILayout.EndHorizontal();
        }

        static public void DoMinMaxField(this Editor editor, string name, string propStartName, string propEndName, float limitMin, float limitMax) {
            editor.DoMinMaxField(name, editor.serializedObject.FindProperty(propStartName), editor.serializedObject.FindProperty(propEndName), limitMin, limitMax);
        }
    }
}
#endif
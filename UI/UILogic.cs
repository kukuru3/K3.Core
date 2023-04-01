using UnityEngine;

namespace K3.UI {
    [RequireComponent(typeof(IUIElement))]
    public class UILogic: MonoBehaviour {

        #pragma warning disable 649
        [SerializeField] string messageID;
        #pragma warning restore 649
        public string MessageID => messageID;

        IUIController logicDestination;

        public IUIController FindUIController() {
            if (logicDestination == null) logicDestination = gameObject.GetComponentInUpwardHierarchy<IUIController>();
            return logicDestination;
        }
    }
}

#if UNITY_EDITOR
namespace K3.Editor {
    using K3.UI;

    using System.Linq;

    using UnityEditor;

    [CustomEditor(typeof(UILogic))]
    class MFDElementLogicEditor : Editor {
        UILogic Target => (UILogic)target;

        public override void OnInspectorGUI() {
            serializedObject.Update();
            var logic = Target.FindUIController();

            if (logic == null) {
                EditorGUILayout.HelpBox("This UI Element is not part of any UI controller. It needs to have one in its parents' hierarchy before you can assign values to it.", MessageType.Warning);
            } else {

                var effType = InferEffectorType();
                if (!effType.HasValue) {
                    EditorGUILayout.HelpBox("This UI Element needs a UI script attached, such as MFDButton, MFDToggle etc.", MessageType.Warning);
                    return;
                }
                var actions = logic.ListActions(effType.Value).ToArray();
                var actionNames = actions.Select(a => a.id).ToArray();
                var msgID = Target.MessageID;
                var index = System.Array.IndexOf(actionNames, msgID);
                index = EditorGUILayout.Popup(index, actionNames);
                if (index >= 0) {
                    serializedObject.FindProperty("messageID").stringValue = actionNames[index];
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private UIEffectorTypes? InferEffectorType() {
            var element = Target.GetComponent<IUIElement>();
            if (element == null) return null;
            return element.EffectorType;
        }
    }
}
#endif

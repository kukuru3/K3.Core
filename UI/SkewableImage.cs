using UnityEngine;
using UnityEngine.UI;

namespace K3.UI {
    class SkewableImage : Image {
        #pragma warning disable 649
        [SerializeField] [Range(-1f, 1f)]float skewY;
        #pragma warning restore 649

        protected override void OnPopulateMesh(VertexHelper vh) { 
            base.OnPopulateMesh(vh);
            var r = GetPixelAdjustedRect();
            var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
            Color32 color32 = color;
            vh.Clear();

            vh.AddVert(new Vector3(v.x, v.y - skewY * r.height), color32, new Vector2(0, 0));
            vh.AddVert(new Vector3(v.x, v.w - skewY * r.height), color32, new Vector2(0, 0));
            vh.AddVert(new Vector3(v.z, v.w + skewY * r.height), color32, new Vector2(0, 0));
            vh.AddVert(new Vector3(v.z, v.y + skewY * r.height), color32, new Vector2(0, 0));
            vh.AddTriangle(0,1,2);
            vh.AddTriangle(2,3,0);
        }
    }
}

#if UNITY_EDITOR
namespace K3.Editor {
    using UnityEditor;
    using K3.UI;
    [CustomEditor(typeof(SkewableImage))]
    class SkewableImageInspector : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skewY"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
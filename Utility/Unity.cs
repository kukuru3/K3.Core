using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace K3 {
    static public class UnityUtils {

        static public bool CheckNormalObjectNull(object o) => o == null;

        static public bool CheckUnityComponentIsNull(object o) {
            var c = o as Component;
            return c == null;
        }

        public static void SetIdentity(this Transform transform, bool alsoScale = false) {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            if (alsoScale) transform.localScale = Vector3.one;
        }

        static public T GetComponentInUpwardHierarchy<T>(this GameObject go, bool parentsOnly = false) {

            bool isUnityComponent = (typeof(Component)).IsAssignableFrom(typeof(T));
            Func<object, bool> isNull = CheckNormalObjectNull;
            if (isUnityComponent) isNull = CheckUnityComponentIsNull;

            for (var t = parentsOnly ? go.transform.parent : go.transform; t != null; t = t.parent) {
                var c = t.GetComponent<T>();
                if (!isNull(c)) return c;
            }
            return default;
        }

        static public Transform[] GetImmediateChildren(this Transform transform) {
            var result = new List<Transform>();
            foreach (Transform child in transform) result.Add(child);
            return result.ToArray();
        }

        static public Transform[] ListUpwardHierarchy(this Transform t0) {
            var result = new List<Transform>();
            for (var t = t0; t != null; t = t.parent) result.Add(t);
            return result.ToArray();
        }

        public static bool IsAnyGenerationParent(GameObject potentialChild, GameObject potentialParent) {
            for (var t = potentialChild.transform.parent; t != null; t = t.parent) {
                if (t.gameObject == potentialParent) return true;
            }
            return false;
        }

        public static Transform FindAnyChild(this Transform from, string name) {
            var children = from.gameObject.GetComponentsInChildren<Transform>(true);
            return children.FirstOrDefault(c => c.name == name);
        }

        static public GameObject FindTagInUpwardHierarchy(this GameObject go, string tag, bool parentsOnly = false) {
            for (var t = parentsOnly ? go.transform.parent : go.transform; t != null; t = t.parent) {
                if (t.tag == tag) return t.gameObject;
            }
            return null;
        }

        static public GameObject FindTagInChildren(this GameObject go, string tag) => go.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject).FirstOrDefault(o => o.tag == tag);

        static public T[] FindAllObjectsOfTypeIncludingInactive<T>() {
            var loadedScenes = GetScenes(loadedOnly: false);
            var gameObjects = loadedScenes.SelectMany(scene => scene.GetRootGameObjects());

            gameObjects = gameObjects.Union(GetDontDestroyOnLoadObjects());
            var components = gameObjects.SelectMany(gameObject => gameObject.GetComponentsInChildren<T>(true)).ToArray();
            return components;
        }

        static GameObject[] GetDontDestroyOnLoadObjects() {
            GameObject temp = null;
            try {
                temp = new GameObject();
                GameObject.DontDestroyOnLoad(temp);
                var dontDestroyOnLoadScene = temp.scene;
                GameObject.DestroyImmediate(temp);
                temp = null;
                return dontDestroyOnLoadScene.GetRootGameObjects();
            } finally {
                if (temp != null) GameObject.DestroyImmediate(temp);
            }
        }

        static public Scene[] GetScenes(bool loadedOnly = false) {
            var result = new List<Scene>();
            for (var i = 0; i < SceneManager.sceneCount; i++) {
                var scene = SceneManager.GetSceneAt(i);
                if (loadedOnly && !scene.isLoaded) continue;
                result.Add(scene);
            }
            return result.ToArray();
        }

        static public string FullObjectPath(GameObject @object) {
            var sb = new StringBuilder();
            for (var t = @object.transform; t != null; t = t.parent) sb.Insert(0, t.name + "/");
            sb.Remove(sb.Length - 1, 1); // remove trailing "/"
            return sb.ToString();
        }

        public static Type FindTypeByTypeName(IEnumerable<System.Reflection.Assembly> assemblies, string typename) {
            foreach (var assembly in assemblies) {
                var t = assembly.GetType(typename);
                if (t != null) return t;
            }

            return null;
        }

        // Can return null if transforms do not share a common ancestor in the hierarchy tree.
        public static Transform FindCommonFrameOfReference(Transform a, Transform b) {
            var hierarchyA = a.ListUpwardHierarchy();
            var hierarchyB = b.ListUpwardHierarchy();

            var lesserOfTwo = Mathf.Min(hierarchyA.Length, hierarchyB.Length);
            Transform lastKnownCommonParent = null;

            for (var i = 0; i < lesserOfTwo; i++) {
                var indexA = hierarchyA.Length - i - 1;
                var indexB = hierarchyB.Length - i - 1;
                if (hierarchyA[indexA] == hierarchyB[indexB]) lastKnownCommonParent = hierarchyA[indexA];
                else break;
            }

            return lastKnownCommonParent;
        }

        public static (float magnitude, Vector2 normalized) Parametrized(this Vector2 vector) {
            var d2 = vector.x * vector.x + vector.y * vector.y;
            var d = Mathf.Sqrt(d2);
            return (d, new Vector2(vector.x / d, vector.y / d));
        }
    }
}
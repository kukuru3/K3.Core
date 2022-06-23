using UnityEngine;
using Coroutine = System.Collections.IEnumerator;

namespace K3.Utility.Coroutines {
    public static class CoroutineUtil {
        private class CoroutineStarter : MonoBehaviour {
            // Coroutine coroutine;
            bool crComplete;
            private void LateUpdate() {
                if (crComplete) 
                    Destroy(gameObject);
            }

            internal void Launch(Coroutine coroutine) {
                StartCoroutine(Wrap(coroutine));
            }

            Coroutine Wrap(Coroutine wrapThis) {
                yield return StartCoroutine(wrapThis);
                crComplete = true;
            }
        }

        public static void Launch(Coroutine coroutine) {
            GameObject go = new GameObject();
            go.name = $"Coroutine starter : {coroutine}";
            var csc = go.AddComponent<CoroutineStarter>();
            csc.Launch(coroutine);
        }
    }
}
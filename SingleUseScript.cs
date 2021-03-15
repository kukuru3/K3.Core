using UnityEngine;

namespace K3 {
    public abstract class SingleUseScript : MonoBehaviour {
        private void Awake() {
            Execute();
        }

        protected abstract void Execute();

        private void LateUpdate() {
            Destroy(this);
            if (transform.childCount == 0) Destroy(gameObject);
        }
    }
}

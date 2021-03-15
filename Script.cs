using UnityEngine;

namespace K3 {
    /// <summary> Intended to REPLACE UNITY MONOBEHAVIOUR!!!!</summary>
    public abstract class Script : MonoBehaviour {

        static internal event System.Action<Script> OnInstantiated;
        static internal event System.Action<Script> OnDestroyed;

        protected virtual void Awake() {
            OnInstantiated?.Invoke(this);
            Init();
        }

        static public bool isApplicationQuitting; 

        private void OnDestroy() {
            OnDestroyed?.Invoke(this);
            Teardown(isApplicationQuitting);
        }

        protected virtual void Init() { }

        protected virtual void Teardown(bool quit) { }

        protected internal virtual void Logic() { }

    }
}

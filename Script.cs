using UnityEngine;

namespace K3 {

    public interface IHasInit {
        bool Initialized { get; }
    }

    /// <summary> Intended to REPLACE UNITY MONOBEHAVIOUR!!!!</summary>
    public abstract class Script : MonoBehaviour, IHasInit {

        static internal event System.Action<Script> OnInstantiated;
        static internal event System.Action<Script> OnDestroyed;

        bool initialized = false;

        protected virtual void Awake() {
            OnInstantiated?.Invoke(this);
            Init();
            initialized = true;
        }

        static internal bool isApplicationQuitting;

        static public bool IsApplicationQuitting => isApplicationQuitting;

        bool IHasInit.Initialized => initialized;

        private void OnDestroy() {
            OnDestroyed?.Invoke(this);
            Teardown(isApplicationQuitting);
        }

        protected virtual void Init() { }

        protected virtual void Teardown(bool quit) { }

        protected internal virtual void Logic() { }
    }
}

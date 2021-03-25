using System;
using System.Collections.Generic;

using UnityEngine;

namespace K3 {
    public class UnityMessagePump : MonoBehaviour, IUnityMessageSource {
        public event Action OnFixedUpdate;
        public event Action OnGUIDrawn;
        public event Action OnLateUpdate;
        public event Action OnUpdate;
        public event Action OnLogic;
        public event Action OnQuitting;

        float timeSinceLastLogic;

        void Awake() {
            DontDestroyOnLoad(gameObject);
        }

        void Update() {
            OnUpdate?.Invoke();
            timeSinceLastLogic += Time.deltaTime;
            if (timeSinceLastLogic >= 1f) {
                timeSinceLastLogic -= 1f;
                OnLogic?.Invoke();
            }
        }

        void LateUpdate() => OnLateUpdate?.Invoke();

        void FixedUpdate() => OnFixedUpdate?.Invoke();

        void OnGUI() => OnGUIDrawn?.Invoke();

        void OnApplicationQuit() => OnQuitting?.Invoke();
    }


    public class K3Scripts : IScriptSource {
        public event Action<Script> OnScriptInstantiated;
        public event Action<Script> OnScriptDestroyed;

        //bool scriptListNeedsCleanup;
        //List<Script> activeScripts = new List<Script>();

        public K3Scripts() {
            Release(); // just in case
            Script.OnDestroyed += HandleScriptDestroyed;
            Script.OnInstantiated += HandleScriptInstantiated;
        }
       
        public void Release() {
            Script.OnDestroyed -= HandleScriptDestroyed;
            Script.OnInstantiated -= HandleScriptInstantiated;
            //activeScripts.Clear();
        }

        private void HandleScriptInstantiated(Script obj) {
            OnScriptInstantiated?.Invoke(obj);
            //activeScripts.Add(obj);
        }

        private void HandleScriptDestroyed(Script obj) {
            //scriptListNeedsCleanup = true;
            OnScriptDestroyed?.Invoke(obj);
        }
    }
}
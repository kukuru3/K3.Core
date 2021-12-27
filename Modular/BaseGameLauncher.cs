using System;

using UnityEngine;

namespace K3._ModularOld {
    //public abstract class BaseGameLauncher : MonoBehaviour, IHasUnityEvents {
    //    public event Action OnFixedUpdate;
    //    public event Action OnGUIDrawn;
    //    public event Action OnLateUpdate;
    //    public event Action OnUpdate;
    //    public event Action OnLogic;
    //    public event Action OnQuitting;
    //    public event Action<Script> OnScriptCreated;
    //    public event Action<Script> OnScriptDestroyed;

    //    protected abstract IGameContext ProvideGameContext();
    //    protected abstract void InitializeApplication();

    //    protected IGameContext Context { get; private set;}

    //    void Awake() {
    //        DontDestroyOnLoad(this); // since something may latch onto our callbacks, do not want to lose them,.
    //        Script.OnInstantiated += HandleScriptInstantiated;
    //        Script.OnDestroyed += HandleScriptDestroyed;
    //        Context = ProvideGameContext();
    //        K3ContextUtilities.Context = Context;
    //        InitializeApplication();
    //    }

    //    private void HandleScriptDestroyed(Script obj) => OnScriptDestroyed?.Invoke(obj);

    //    private void HandleScriptInstantiated(Script obj) {
    //        OnScriptCreated?.Invoke(obj);
    //    }

    //    /// <summary> Use this if you want a simple context with all the functionalities.</summary>
    //    public IGameContext ProvideDefaultContext() {
    //        var exe = new ExecutionController();

    //        var c = new SimpleContext() {
    //            UnityEventSource = this,
    //            ModuleContainer = new ModuleContainer(),
    //            Executor = exe,
    //        };

    //        exe.Initialize(c);

    //        return c;
    //    }

    //    float timeSinceLastLogic;

    //    void Update() {
    //        OnUpdate?.Invoke();
    //        timeSinceLastLogic += Time.deltaTime;
    //        if (timeSinceLastLogic >= 1f) {
    //            timeSinceLastLogic -= 1f;
    //            OnLogic?.Invoke();
    //        }
    //    }

    //    protected virtual void CleanupApplication() { }
        
    //    protected void LateUpdate() => OnLateUpdate?.Invoke();
        
    //    protected void FixedUpdate() => OnFixedUpdate?.Invoke();
        
    //    protected void OnGUI() => OnGUIDrawn?.Invoke();

    //    protected void OnApplicationQuit() {
    //        CleanupApplication();
    //        OnQuitting?.Invoke();
    //        Context.Executor.Teardown();
    //        Context.ModuleContainer.RemoveModule(Context.ModuleContainer.RootModule);
    //    }
    //}
}

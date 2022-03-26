using System.Collections.Generic;

namespace K3.Modules {
    public interface IExecutesInitialization {
        void Initialize();
    }

    public interface IExecutesFrame {
        void Frame();
    }

    public interface IExecutesLateUpdate {
        void LateUpdate();
    }

    public interface IExecutesTick {
        void Tick();
    }

    public interface IExecutesLogic {
        void Logic();
    }

    public interface IExecutesGUI {
        void ExecuteUGUI();
    }

    public interface IActivatable {
        bool Active { get; set; }
    }

    public interface IExecutesTeardown {
        void Teardown();
    }

    public interface IExecutesApplicationQuit {
        void ApplicationWillQuit();
    }

    //public interface IGameContext {
    //    IHasUnityEvents UnityEventSource { get; }
    //    IAppModuleContainer ModuleContainer { get; }
    //    ISceneContainers Containers { get; }
    //    ExecutionController Executor { get; }
    //}

    //public interface IGameLauncher {
    //    void LaunchGame(IHasUnityEvents eventSource);
    //}

    //[System.Obsolete("Replace the launcher / bridge system with a more modular services-on-demand")]
    //public interface IHasUnityEvents {
    //    event System.Action OnFixedUpdate;
    //    event System.Action OnGUIDrawn;
    //    event System.Action OnLateUpdate;
    //    event System.Action OnUpdate;
    //    event System.Action OnLogic;
    //    event System.Action OnQuitting;
    //    event System.Action<Script> OnScriptCreated;
    //    event System.Action<Script> OnScriptDestroyed;
    //}

    //public interface IAppModule {
    //    void OnWasInsertedIntoModuleContext();
    //    void OnWasRemovedFromModuleContext();
    //    event System.Action<IAppModule> WillBeDestroyed;
    //}

    //public interface IDataContainer {
    //}

    //public interface IAppModuleContainer {
    //    void LaunchModule(IAppModule module, IAppModule parent = null);
    //    void RemoveModule(IAppModule module);
    //    IEnumerable<IAppModule> AllModules { get; }
    //    T Get<T>() where T: IAppModule;
    //    IAppModule RootModule { get; }
    //    event System.Action ModulesChanged;
    //}

    /// <summary>Something that provides a registered instance of a type T given the type T</summary>
    public interface IServiceContainer {
        T Get<T>();
        T TryGet<T>();
        IEnumerable<T> Implementing<T>();
    }

    public interface IServiceBinder {
        void Bind(object service);
        void UnbindAll<T>();
        void Bind<T>(T service);
    }
}

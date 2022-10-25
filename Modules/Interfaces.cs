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

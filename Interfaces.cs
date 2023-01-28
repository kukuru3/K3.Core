using System.Collections.Generic;

namespace K3 {

    public interface IAutoTracked<T> {

    }

    public interface IUnityMessageSource {
        event System.Action OnFixedUpdate;
        event System.Action OnGUIDrawn;
        event System.Action OnLateUpdate;
        event System.Action OnUpdate;
        event System.Action OnLogic;
        event System.Action OnQuitting;
    }

    public interface IScriptSource {
        event System.Action<Script> OnScriptInstantiated;
        event System.Action<Script> OnScriptDestroyed;
    }

    public interface ISceneContainers {
        IContainer<T> GetContainer<T>();
        void RegisterContainer<T>(IContainer<T> container);
        void RemoveContainer<T>(IContainer<T> container);
    }

    public interface IContainer {
        void Add(object item);
        void Remove(object item);
    }

    public interface IContainer<T> : IContainer {
        IEnumerable<T> AllElements { get; }
        void Add(T item);
        void Remove(T item);
        bool Contains(T item);

        void ExecutePerItem(System.Action<T> toExecute, System.Action<T> onRemoval = null);
    }
}

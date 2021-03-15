using System.Collections.Generic;

namespace K3 {

    public interface IAutoTracked<T> {

    }

    public interface ISceneContainers {
        IContainer<T> GetContainer<T>();
        void RegisterContainer<T>(IContainer<T> container);
    }

    public interface IContainer {
        void Add(object item);
        void Remove(object item);
    }

    public interface IContainer<T> : IContainer {
        IEnumerable<T> AllElements { get; }

        void Add(T item);
        void Remove(T item);

        void ExecutePerItem(System.Action<T> toExecute, System.Action<T> onRemoval = null);
    }
}

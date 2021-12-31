namespace K3.Modules {
    public interface IModuleBehaviour {
        void InjectContext(IAppModule module);
    }

    public abstract class ModuleBehaviour<TModule> : UnityEngine.MonoBehaviour, IModuleBehaviour where TModule : IAppModule {
        protected TModule Module { get; private set; }

        protected T GetModuleComponent<T>() => Module.GetModuleComponent<T>();

        void IModuleBehaviour.InjectContext(IAppModule module) {
            Module = (TModule)module;
            Launch();
        }

        void OnDestroy() {
            Teardown();
        }

        /// <summary>Context has been injected and we are ready to execute initialization logic</summary>
        protected virtual void Launch() { } 

        protected virtual void Teardown() { }
    }

}

namespace K3.Modules {
    public interface IModuleBehaviour {
        void InjectContext(BaseModule context);
    }

    public abstract class ModuleBehaviour<TModule> : UnityEngine.MonoBehaviour, IModuleBehaviour  where TModule : BaseModule {
        protected TModule Module { get; private set; }

        void IModuleBehaviour.InjectContext(BaseModule module) {
            Module = (TModule)module;
            Launch();
        }

        /// <summary>Context has been injected and we are ready to execute initialization logic</summary>
        protected virtual void Launch() { } 
    }

}

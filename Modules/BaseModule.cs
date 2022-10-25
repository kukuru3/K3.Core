namespace K3.Modules {
    public abstract class BaseComponent {
        protected internal abstract void InjectModule(BaseModule module);
        protected internal abstract void Release();
    }
    
    public abstract class Component<TModule> : BaseComponent where TModule : BaseModule
        {
        protected TModule Module { get; private set; }

        protected T GetModuleComponent<T>() => Module.GetModuleComponent<T>();

        protected internal override void InjectModule(BaseModule module) {
            if (module is TModule typedModule) Module = typedModule;
            else throw new System.InvalidCastException($"{GetType().Name} expects a module of type {typeof(TModule).Name}; {module.GetType().Name} was supplied");
            Launch();
        }

        protected internal override void Release() {
            Teardown();
        }

        protected virtual void Launch() { }

        protected virtual void Teardown() { }
    }

    public interface IService<TModule> where TModule : BaseModule {
        TModule Module { get; set; }
    }

}

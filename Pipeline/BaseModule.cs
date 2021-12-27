﻿namespace K3.Modules {
    public abstract class BaseComponent {
        protected internal abstract void InjectModule(BaseModule module);
    }
    
    public abstract class Component<TModule> : BaseComponent where TModule : BaseModule
        {
        protected TModule Module { get; private set; }

        protected internal override void InjectModule(BaseModule module) {
            if (module is TModule typedModule) Module = typedModule;
            else throw new System.InvalidCastException($"{GetType().Name} expects a context of type {typeof(TModule).Name}; {module.GetType().Name} was supplied");
            Launch();
        }

        protected virtual void Launch() { }
    }

    public interface IService<TModule> where TModule : BaseModule {
        TModule Module { get; set; }
    }

}

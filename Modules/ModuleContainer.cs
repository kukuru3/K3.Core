using System;
using System.Collections.Generic;

namespace K3.Modules {

    public interface IModuleContainer {
        IEnumerable<BaseModule> Modules { get; }

        T GetModule<T>();

        BaseModule GetModule(System.Type t);

        void InstallModule(BaseModule module);
        void InstallModule<T>() where T : BaseModule, new();
        void Clear();
        void RemoveModule(BaseModule module);
    }

    internal class ModuleContainer : IModuleContainer {
        Locators.SimpleLocator moduleLocator = new Locators.SimpleLocator(); 
        List<BaseModule> modules = new List<BaseModule>();
        Dictionary<Type, BaseModule> lookupCache = new();

        public IEnumerable<BaseModule> Modules => modules;

        public void Clear() {
            foreach (var module in modules.ToArray()) module.DestroyModule();
            modules.Clear();
        }

        void IModuleContainer.InstallModule(BaseModule module) {
            this.modules.Add(module);
            moduleLocator.Register(module);
            module.InjectContainer(this);
            InvalidateLookupCache(); // not strictly necessary but eh.
        }

        void IModuleContainer.InstallModule<T>() => ((IModuleContainer)this).InstallModule(new T());

        public void RemoveModule(BaseModule module) {
            if (modules.Remove(module)) { 
                moduleLocator.Unregister(module);
                module.DestroyModule();
                InvalidateLookupCache();
            }
        }


        public T GetModule<T>() => moduleLocator.Locate<T>() ?? default;
        
        public BaseModule GetModule(Type t) {
            if (!lookupCache.TryGetValue(t, out var value))
                lookupCache[t] = value = (BaseModule)moduleLocator.Locate(t);
            
            return value;
        }

        private void InvalidateLookupCache() => lookupCache.Clear();

    }
}
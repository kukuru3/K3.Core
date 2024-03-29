using System;
using System.Collections.Generic;

namespace K3.Modules {

    public interface IModuleContainer {
        IEnumerable<BaseModule> Modules { get; }

        T GetModule<T>();

        BaseModule GetModule(System.Type t);

        void InstallModule(BaseModule module);
        T CreateAndLaunchModule<T>() where T : BaseModule, new();
        void Clear();
        void RemoveModule(BaseModule module);
    }

    internal class ModuleContainer : IModuleContainer {
        Locators.SimpleLocator moduleLocator = new Locators.SimpleLocator(); 
        List<BaseModule> modules = new List<BaseModule>();
        Dictionary<Type, BaseModule> lookupCache = new Dictionary<Type, BaseModule>();

        List<BaseModule> safeCopy;
        
        public IEnumerable<BaseModule> Modules { get {
            // this roundabout thing is so we can foreach 
            // over the modules
            safeCopy ??= new List<BaseModule>(modules);
            return safeCopy;
        } }

        public void Clear() {
            foreach (var module in modules.ToArray()) module.DestroyModule();
            modules.Clear();
        }

        void IModuleContainer.InstallModule(BaseModule module) {
            UnityEngine.Debug.Log($"<color=#a0f0a0><b>Creating module</b></color> : {module.GetType()}");
            this.modules.Add(module);
            moduleLocator.Register(module);
            module.InjectContainer(this);
            InvalidateLookupCache(); // not strictly necessary but eh.
        }

        T IModuleContainer.CreateAndLaunchModule<T>() {
            var module = new T();
            ((IModuleContainer)this).InstallModule(module);
            return module;
        }

        public void RemoveModule(BaseModule module) {
            if (modules.Remove(module)) { 
                UnityEngine.Debug.Log($"<color=#f0a070><b>Removing module</b></color> : {module.GetType()}");
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

        private void InvalidateLookupCache() {
            lookupCache.Clear();
            safeCopy = null;
        }
    }
}
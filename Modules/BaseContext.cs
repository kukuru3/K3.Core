using System;
using System.Collections.Generic;

namespace K3.Modules {
    public interface IAppModule {
        T GetModuleComponent<T>();
    }
    public abstract class BaseModule : IAppModule {

        List<BaseComponent> components = new List<BaseComponent>();
        Locators.ILocator componentLocator = new Locators.SimpleLocator();
        protected IEnumerable<BaseComponent> AllComponents => componentLocator.LocateAll<BaseComponent>();
        protected IModuleContainer Container { get; private set; } 


        public T CreateComponent<T>() where T: BaseComponent, new() {
            var c = new T();
            AddComponent(c);
            return c;
        }
        public void AddComponent(BaseComponent m) {
            components.Add(m);
            componentLocator.Register(m);
            m.InjectModule(this);
        }

        public IEnumerable<T> ListComponents<T>() {
            return componentLocator.LocateAll<T>();
            // foreach (var m in components) if (m is T tm) yield return tm;
        }

        public T GetModuleComponent<T>() {
            return componentLocator.Locate<T>();
            // foreach (var m in components) if (m is T tm) return tm;
            // return default;
        }

        internal void InjectContainer(ModuleContainer container) {
            this.Container = container;
            ValidateState();
            Launch();
        }

        protected virtual void ValidateState() { }

        internal void DestroyModule() {
            Teardown();
            ReleaseComponents();
            this.Container = null;
        }

        private void ReleaseComponents() {
            foreach (var component in this.components) component.Release();
            components.Clear();
            // at the moment, Application.quitting calls Teardown for modules
            // then, the temporary scene game objects are destroyed and their Teardown(), if any, gets called
            // so we might want to leave componentLocator undestroyed at this stage, I suppose.
            // componentLocator = null; 
        }

        protected virtual void Launch() { }

        protected virtual void Teardown() { }

        protected virtual void DoTick() { }
    }
}

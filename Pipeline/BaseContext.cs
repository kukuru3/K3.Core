using K3._ModularOld;

using System;
using System.Collections.Generic;

namespace K3.Modules {
    public interface IAppModule {
        T GetModuleComponent<T>();
    }
    public abstract class BaseModule : IAppModule, IExecutesFrame, IExecutesLateUpdate, IExecutesTick {

        List<BaseComponent> components = new List<BaseComponent>();
        Locators.ILocator componentLocator = new Locators.SimpleLocator();
        protected IEnumerable<BaseComponent> AllComponents => componentLocator.LocateAll<BaseComponent>();
        protected IModuleContainer Container { get; private set; } 

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
            this.Container = null;
        }

        protected virtual void Launch() { }

        protected virtual void Teardown() { }

        void IExecutesTick.Tick() { foreach (var module in components) if (module is IExecutesTick t) t.Tick(); }
        void IExecutesLateUpdate.LateUpdate() { foreach (var module in components) if (module is IExecutesLateUpdate lu) lu.LateUpdate(); }
        void IExecutesFrame.Frame() { foreach (var module in components) if (module is IExecutesFrame f) f.Frame(); }

        protected virtual void DoTick() { }
    }
}

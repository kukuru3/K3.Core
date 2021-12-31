using K3._ModularOld;

using System.Collections.Generic;

namespace K3.Modules {
    public interface IAppModule {
        T GetModuleComponent<T>();
    }
    public abstract class BaseModule : IAppModule, IExecutesFrame, IExecutesLateUpdate, IExecutesTick {

        List<BaseComponent> components = new List<BaseComponent>();
        protected IEnumerable<BaseComponent> AllModules => components;

        protected IGlobalContext GlobalContext { get; private set; } 

        public void AddComponent(BaseComponent m) {
            components.Add(m);
            m.InjectModule(this);
        }

        public T GetModuleComponent<T>() {
            foreach (var m in components) if (m is T tm) return tm;
            return default;
        }

        internal void InjectGlobalContext(GlobalContext context) {
            this.GlobalContext = context;
            Launch();
        }

        internal void DestroyModule() {
            Teardown();
            this.GlobalContext = null;
        }

        protected virtual void Launch() { }

        protected virtual void Teardown() { }

        void IExecutesTick.Tick() { foreach (var module in components) if (module is IExecutesTick t) t.Tick(); }
        void IExecutesLateUpdate.LateUpdate() { foreach (var module in components) if (module is IExecutesLateUpdate lu) lu.LateUpdate(); }
        void IExecutesFrame.Frame() { foreach (var module in components) if (module is IExecutesFrame f) f.Frame(); }

        protected virtual void DoTick() { }
    }

}

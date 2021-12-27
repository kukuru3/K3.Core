using K3._ModularOld;

using System.Collections.Generic;

namespace K3.Modules {
    public abstract class BaseModule : IExecutesFrame, IExecutesLateUpdate, IExecutesTick {
        protected internal virtual void Cleanup() { }

        List<BaseComponent> modules = new List<BaseComponent>();
        protected IEnumerable<BaseComponent> AllModules => modules;

        protected IGlobalContext GlobalContext { get; private set; } 

        public void AddModule(BaseComponent m) {
            modules.Add(m);
            m.InjectModule(this);
        }

        public T GetModule<T>() where T : BaseComponent {
            foreach (var m in modules) if (m is T tm) return tm;
            return default;
        }

        internal void InjectGlobalContext(GlobalContext context) {
            this.GlobalContext = context;
            Launch();
        }

        protected virtual void Launch() { }

        void IExecutesTick.Tick() { foreach (var module in modules) if (module is IExecutesTick t) t.Tick(); }
        void IExecutesLateUpdate.LateUpdate() { foreach (var module in modules) if (module is IExecutesLateUpdate lu) lu.LateUpdate(); }
        void IExecutesFrame.Frame() { foreach (var module in modules) if (module is IExecutesFrame f) f.Frame(); }

        protected virtual void DoTick() { }
    }

}

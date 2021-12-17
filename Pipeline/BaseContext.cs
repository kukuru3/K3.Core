using K3.Modular;

using System.Collections.Generic;

namespace K3.Pipeline {
    public abstract class BaseContext : IExecutesFrame, IExecutesLateUpdate, IExecutesTick {
        protected internal virtual void Cleanup() { }

        List<BaseModule> modules = new List<BaseModule>();
        protected IEnumerable<BaseModule> AllModules => modules;

        protected IGlobalContext GlobalContext { get; private set; } 

        public void AddModule(BaseModule m) {
            modules.Add(m);
            m.InjectContext(this);
        }

        internal void InjectContext(GlobalContext context) {
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

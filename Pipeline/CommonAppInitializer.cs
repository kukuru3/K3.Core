using K3.Modules;
using K3.Pipeline;

namespace Embers.Program {
    public abstract class CommonAppInitializer : IPipelineInjector {

        protected IModuleContainer Container { get; private set; }
        protected IPipeline Pipeline { get; private set; }
        public void Inject(IPipeline pipeline) {

            Pipeline = pipeline;
            Pipeline.RegisterMethod(IPipeline.Triggers.AppStart, LaunchGame);
            Pipeline.RegisterMethod(IPipeline.Triggers.Teardown, ClearContext);
        }

        // you can (and should) register hooks of your own.
        private void RegisterLoopHoks(ModuleContainer container) {
            var callbackHooks = new ContainerCallbackHoooks(container);

            Pipeline.RegisterMethod(IPipeline.Triggers.Update, callbackHooks.Frame);
            Pipeline.RegisterMethod(IPipeline.Triggers.LateUpdate, callbackHooks.LateUpdate);
            Pipeline.RegisterMethod(IPipeline.Triggers.FixedUpdate, callbackHooks.Tick);
        }

        void LaunchGame() {
            var container = new ModuleContainer();
            Container = container;
            RegisterLoopHoks(container);
            InitializeApplication(container);
        }
        protected abstract void InitializeApplication(IModuleContainer context);

        protected virtual void ClearContext() {
            Container.Clear();
            Container = null;
        }
    }

}

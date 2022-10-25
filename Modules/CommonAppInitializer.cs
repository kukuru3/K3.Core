using K3.Modules;

namespace K3.Pipeline {
    public abstract class CommonAppInitializer : IPipelineInjector {

        protected IModuleContainer ModuleContainer { get; private set; }
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
            UnityEngine.Debug.Log("Launch game");
            var container = new ModuleContainer();
            ModuleContainer = container;

            ContextUtility.SetContainerInstance(container);

            RegisterLoopHoks(container);
            InitializeApplication(container);
        }
        protected abstract void InitializeApplication(IModuleContainer context);

        protected virtual void ClearContext() {
            ModuleContainer.Clear();
            ModuleContainer = null;
        }
    }

}

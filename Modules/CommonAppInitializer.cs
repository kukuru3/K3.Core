using K3.Modules;

namespace K3.Pipeline {
    public abstract class CommonAppInitializer : IPipelineInjector {

        protected IModuleContainer ModuleContainer { get; private set; }
        protected IPipeline Pipeline { get; private set; }
        public void Inject(IPipeline pipeline) {
            
            Pipeline = pipeline;
            Pipeline.RegisterMethod(Triggers.AppStart, LaunchGame);
            Pipeline.RegisterMethod(Triggers.Teardown, ClearContext);
        }

        // you can (and should) register hooks of your own.
        private void RegisterLoopHoks(ModuleContainer container) {
            var callbackHooks = new ContainerCallbackHoooks(container);

            Pipeline.RegisterMethod(Triggers.Update, callbackHooks.Frame);
            Pipeline.RegisterMethod(Triggers.LateUpdate, callbackHooks.LateUpdate);
            Pipeline.RegisterMethod(Triggers.FixedUpdate, callbackHooks.Tick);
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

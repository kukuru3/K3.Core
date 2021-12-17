using K3.Modular;
using K3.Pipeline;

namespace Embers.Program {
    public abstract class CommonAppInitializer : IPipelineInjector {

        protected IGlobalContext Context { get; private set; }
        protected IPipeline Pipeline { get; private set; }
        public void Inject(IPipeline pipeline) {

            this.Pipeline = pipeline;
            pipeline.RegisterMethod(IPipeline.Triggers.AppStart, LaunchGame);
            pipeline.RegisterMethod(IPipeline.Triggers.Teardown, ClearContext);
        }

        // you can (and should) register hooks of your own.
        private void RegisterLoopHoks(GlobalContext context) {
            var callbackHooks = new CallbackHooks(context);

            Pipeline.RegisterMethod(IPipeline.Triggers.Update, callbackHooks.Frame);
            Pipeline.RegisterMethod(IPipeline.Triggers.LateUpdate, callbackHooks.LateUpdate);
            Pipeline.RegisterMethod(IPipeline.Triggers.FixedUpdate, callbackHooks.Tick);
        }

        void LaunchGame() {
            var context = new GlobalContext();
            GlobalContextHolder.GlobalContext = context;
            Context = context;
            RegisterLoopHoks(context);
            InitializeApplication(GlobalContextHolder.GlobalContext);
        }
        protected abstract void InitializeApplication(IGlobalContext context);

        protected virtual void ClearContext() {
            GlobalContextHolder.GlobalContext.Clear();
            GlobalContextHolder.GlobalContext = null;
        }
    }

}

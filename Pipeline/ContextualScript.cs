namespace K3.Pipeline {
    public interface IContextualScript {
        void InjectContext(BaseContext context);
    }

    public abstract class ContextualScript<TSegment> : UnityEngine.MonoBehaviour, IContextualScript  where TSegment : BaseContext {
        protected TSegment Context { get; private set; }

        void IContextualScript.InjectContext(BaseContext context) {
            Context = (TSegment)context;
            Launch();
        }

        /// <summary>Context has been injected and we are ready to execute initialization logic</summary>
        protected virtual void Launch() { } 
    }

}

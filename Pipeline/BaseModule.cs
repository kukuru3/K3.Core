namespace K3.Pipeline {
    public abstract class BaseModule {
        protected internal abstract void InjectContext(BaseContext context);
    }
    
    public abstract class Module<TSegment> : BaseModule where TSegment : BaseContext {
        protected TSegment Context { get; private set; }

        protected internal override void InjectContext(BaseContext context) {
            if (context is TSegment typedContext) Context = typedContext;
            else throw new System.InvalidCastException($"{GetType().Name} expects a context of type {typeof(TSegment).Name}; {context.GetType().Name} was supplied");
            Launch();
        }

        protected virtual void Launch() { }
    }

    public interface IService<TContext> where TContext : BaseContext {
        TContext Context { get; set; }
    }

}

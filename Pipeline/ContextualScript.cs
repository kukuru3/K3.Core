namespace K3.Modules {
    public interface IModuleBehaviour {
        void InjectModule(IAppModule module);
    }

    public abstract class ModuleBehaviour<TModule> : UnityEngine.MonoBehaviour, IModuleBehaviour where TModule : IAppModule {
        protected TModule Module { get; private set; }

        protected T GetModuleComponent<T>() {
            var c = Module.GetModuleComponent<T>();
            if (c == null) throw new System.Exception($"{this} wants module {Module} to provide component of type `{typeof(T).Name}`, but none were declared");
            return c;
        }

        void IModuleBehaviour.InjectModule(IAppModule module) {
            Module = (TModule)module;
            Launch();
        }

        void OnDestroy() {
            Teardown();
        }

        /// <summary>Context has been injected and we are ready to execute initialization logic</summary>
        protected virtual void Launch() { } 

        protected virtual void Teardown() { }
    }

    public static class ContextUtility {

        static IModuleContainer container;

        static internal void SetContainerInstance(IModuleContainer c) {
            container = c;
        }

        public static void InjectModulesIntoBehaviours(UnityEngine.GameObject root) {
            if (container == null) UnityEngine.Debug.LogError("Cannot inject modules since container is not set");
            var cmps = root.GetComponentsInChildren<IModuleBehaviour>(true);

            // try {
                foreach (var cmp in cmps) {
                    var ctx = InferModuleForScript(cmp);
                    if (ctx != null) {
                        cmp.InjectModule(ctx);
                    } else {
                        var arg = cmp?.GetType()?.BaseType?.GetGenericArguments()?[0];
                        UnityEngine.Debug.LogWarning($"Was not able to inject the context into {cmp},  expected module of type {arg} but one does not exist");
                        (cmp as UnityEngine.MonoBehaviour).enabled = false; // needs to be disabled so it doesn't emit Update() and similar callbacks
                    }
                }
            //}
            //catch (System.Exception) {
            //    UnityEngine.Debug.LogWarning("Context injection errors");
            //}
        }

        static BaseModule InferModuleForScript(IModuleBehaviour script) {


            System.Type[] genericArguments = System.Array.Empty<System.Type>();

            var type = script.GetType();

            var genericModuleBehaviourType = typeof(ModuleBehaviour<>);
            var ownModuleBehaviourType = ReflectionUtility.GetTypeInInheritanceHierarchyThatIsImplementationOfRawGeneric(type, genericModuleBehaviourType);
            if (ownModuleBehaviourType != null) {
                var q = ownModuleBehaviourType.GetGenericArguments()[0];
                return container.GetModule(q);
            }
            return default;

            //while (type != null && genericArguments.Length == 0) {
            //    type = type.BaseType;
            //    genericArguments = type.GetGenericArguments();
            //}
            //if (genericArguments.Length == 0)
            //    throw new System.Exception("Could not find the module type");

            //var q = genericArguments[0];

            //foreach (var module in container.Modules)
            //    if (module.GetType().IsAssignableFrom(q))
            //        return module;

            //return default;
        }
    }
}

// new architecture: 

// - MULTICONTEXT: 
// - instead of having only one context, we can have multiple ones.
// - contexts exist in parallel, are not in any mutual hierarchical relationship,
// and may locate each other's references
// - contexts can be created and destroyed during runtime

// - view layer scripts should be context-bound
// - destroying a context should remove all view elements bound to that context.

// - MasterContext would contain crucial global references such as data repository, unity message pump, etc.
// - MenuContext would contain menu related stuff
// - CampaignContext would contain a few services related to campaign data persistence
// - DungeonContext would contain pretty much all the data we have in module containers right now.
// - MapGeneratorContext would contain everything needed for map generation

// major game state changes / loads / transitions are driven by context creation and destruction.
// context creation can initiate loading and view generator 

// each context may have its own module container.


// initialization / bootstrap shenanigans:
// in a build, we will only have the bootstrapper and no other objects in the scene.

// builders / loadscripts will then build scenes, which will include instantiation of context objects.
// All scene build instantiations will be regulated and so bootstrapper / initializers will know that these 
// objects have been instantiated and are thus able to inject the proper context into them.

// consider to have a ContextInjectionMarker monobehaviour to "mark" these objects so we can avoid a GetComponentsInChildren<>
// (the ContextInjectionMarker might do the GetComponents... but that's okay)

// HOWEVER, in the editor scenes, we will have the bootstrapper and some objects already present in the scene.

// additionally, we must handle DISABLED OBJECTS, either those instantiated as disabled, or those present in the scene as disabled.
// I vote that we inject the context in those as well. 
// Their initialization / insertion into containers can wait.

// In order for a script to INITIALIZE, the following must be true:
// - the script must be active in hierarchy
// - the script has not initialized before
// - the script has been injected with context
// - ideally, the LOADING STAGE for this context has completed.

// if ContextualScripts rely on a special initializer, ALL callbacks need to start with an "if (!initialized) return;"
// otherwise they might execute before the item is initialized.
// This might be a problem since a LOT of them use Update, LateUpdate and FixedUpdate.
// wrapping these callbacks is also not acceptable because then they we get an extra call stack item and a vtable performance hit.
// the only way to avoid all this is to rely on Start() after all. But then you must GUARANTEE that the context will be injected
// before Start() into these objects. Furthermore, the context might have not finished initializing or loading, so ContextualScripts
// that rely on, say, containers existing and being populated, will fail

// Therefore: 
// - any instantiation takes place through a wrapper which checks for ContextMarker
// - any scene objects that are in the scene have a PreexistingSceneObjectContextInjectionMarker

// there are 4 classes of context initialization that need to be handled:
// - objects Instantiate'd as part of a load process (Load Instantiated)
// - objects Instantiate'd after a load process, say, during gameplay (Gameplay Instantiated)
// - preexisting scene objects in a scene, when the scene is being loaded by our code (Preexisting Scene)
// - preexisting scene objects in a scene, when the scene is directly launched from the Unity Editor. (Grandfathered)

// - context injection step for OBJECTS UNDERGOING CONTROLLED INSTANTIATION *must* occur before the instantiated objects are given a chance to Awake() or Start()
// - context injection step for GRANDFATHERED-IN OBJECTS should occur during Bootstrapper Awake(). The grandfathered-in objects must at that point be located through PreexistingMarker,
// most likely using FindObjectsOfTypeIncludingInactive. This is an expensive search, but only ever done once.
// - what happens with preexisting objects which are part of a scene load that occurs after bootstrap Awake()?

// Consider: do not have a BOOTSTRAPPER in every scene.
// instead, look into solving editor-specific edge cases by DETECTING EDITOR LAUNCH (via events; attributes?) AND AUTOMATICALLY GENERATING AN EDITOR-AWARE BOOTSTRAPPER
// entry scene of course still exists, and still has the regular bootstrapper object and nothing else.

using K3.Modules;

using System.Collections.Generic;

namespace K3.Pipeline {

    class ContainerCallbackHoooks {

        readonly Modules.ModuleContainer moduleHolder;
        public ContainerCallbackHoooks(Modules.ModuleContainer context) {
            this.moduleHolder = context;
        }

        void Propagate<T>(System.Action<T> propagatedAction) {
            foreach (var module in moduleHolder.Modules) {
                if (module is T tmodule) {
                    propagatedAction(tmodule);
                }
                foreach (var cmp in module.ListComponents<T>())
                    propagatedAction(cmp);
            }
        }

        public void Frame() {
            Propagate<IExecutesFrame>(f => f.Frame());
            //foreach (var module in moduleHolder.Modules) if (module is IExecutesFrame framer) framer.Frame();
        }

        public void LateUpdate() {
            Propagate<IExecutesLateUpdate>(ielu => ielu.LateUpdate());
            // foreach (var ctx in moduleHolder.Modules) if (ctx is IExecutesLateUpdate framer) framer.LateUpdate();
        }

        public void Tick() {
            Propagate<IExecutesTick>(ticker => ticker.Tick());
            // foreach (var ctx in moduleHolder.Modules) if (ctx is IExecutesTick ticker) ticker.Tick();
        }

    }

}


namespace K3.Modules {

    public interface IModuleContainer {
        IEnumerable<BaseModule> Modules { get; }

        T GetModule<T>();

        BaseModule GetModule(System.Type t);

        void InstallModule(BaseModule module);
        void InstallModule<T>() where T : BaseModule, new();
        void Clear();
        void RemoveModule(BaseModule module);
    }

    internal class ModuleContainer : IModuleContainer {
        Locators.SimpleLocator moduleLocator = new Locators.SimpleLocator(); 
        List<BaseModule> modules = new List<BaseModule>();

        public IEnumerable<BaseModule> Modules => modules;

        public void Clear() {
            foreach (var module in modules.ToArray()) module.DestroyModule();
            modules.Clear();
        }

        public ModuleContainer() {

        }

        void IModuleContainer.InstallModule(BaseModule module) {
            this.modules.Add(module);
            moduleLocator.Register(module);
            module.InjectContainer(this);
        }

        void IModuleContainer.InstallModule<T>() => ((IModuleContainer)this).InstallModule(new T());

        public void RemoveModule(BaseModule module) {
            if (modules.Remove(module)) { 
                moduleLocator.Unregister(module);
                module.DestroyModule();
            }
        }

        public T GetModule<T>() => moduleLocator.Locate<T>() ?? default;
        public BaseModule GetModule(System.Type t) => (BaseModule)moduleLocator.Locate(t);
    }
}
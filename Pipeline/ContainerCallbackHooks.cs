// new architecture: 

// - MULTICONTEXT: 
// - multiple modules exist in parallel, with no direct mutual relationship
// - dependencies are possible between modules
// - modules can be created and destroyed during runtime

// - view layer scripts should be module-bound
// - destroying a module should remove all view elements bound to that module.

// - MasterContext would contain crucial global references such as data repository, unity message pump, etc.
// - MenuContext would contain menu related stuff
// - CampaignContext would contain a few services related to campaign data persistence
// - DungeonContext would contain pretty much all the data we have in module containers right now.
// - MapGeneratorContext would contain everything needed for map generation

// major game state changes / loads / transitions are driven by context creation and destruction.
// context creation can initiate loading and view generator 

// each context may have its own component container.

// initialization / bootstrap shenanigans:

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

namespace K3.Pipeline {

    class ContainerCallbackHoooks {

        readonly ModuleContainer moduleHolder;
        
        public ContainerCallbackHoooks(ModuleContainer context) {
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

        internal void Frame() {
            Propagate<IExecutesFrame>(f => f.Frame());
        }

        internal void LateUpdate() {
            Propagate<IExecutesLateUpdate>(ielu => ielu.LateUpdate());
        }

        internal void Tick() {
            Propagate<IExecutesTick>(ticker => ticker.Tick());
        }

    }

}
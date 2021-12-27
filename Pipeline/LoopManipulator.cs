using System;
using System.Linq;

using UnityEngine.LowLevel;

namespace K3.Pipeline {

    static class LoopManipulator {
        static int FindIndexOf<T>(PlayerLoopSystem[] arr) {
            if (arr == null) return -1;
            for (var i = 0; i < arr.Length; i++) if (arr[i].type == typeof(T)) return i;
            return -1;
        }

        internal static void AddLoopEvents() {
            
            Insert<UnityEngine.PlayerLoop.Update.ScriptRunBehaviourUpdate, ModulesUpdate>(
                () => CustomPipeline.Execute(IPipeline.Triggers.Update)
            );

            Insert<UnityEngine.PlayerLoop.PreLateUpdate.ScriptRunBehaviourLateUpdate, ModulesLateUpdate>(
                () => CustomPipeline.Execute(IPipeline.Triggers.LateUpdate)
            );

            Insert<UnityEngine.PlayerLoop.FixedUpdate.ClearLines, ModulesFixedUpdate>(
                () => CustomPipeline.Execute(IPipeline.Triggers.FixedUpdate)
            );

            Insert<UnityEngine.PlayerLoop.PostLateUpdate.ProfilerEndFrame, PostCameraRender>(
                () => CustomPipeline.Execute(IPipeline.Triggers.PostRender),
                true
            );
        }

        private static void Insert<TMatch, TInserted>(PlayerLoopSystem.UpdateFunction action, bool insertAfter = false) {
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            for (var i = 0; i < loop.subSystemList.Length; i++) {
                var indx = FindIndexOf<TMatch>(loop.subSystemList[i].subSystemList);
                if (indx >= 0) {
                    Insert(
                        ref loop.subSystemList[i].subSystemList,
                        indx + (insertAfter ? 1 : 0),
                        typeof(TInserted),
                        action
                    );
                    break;
                }
            }
            PlayerLoop.SetPlayerLoop(loop);
        }

        internal static void ClearEvents() {
            PlayerLoop.SetPlayerLoop(PlayerLoop.GetDefaultPlayerLoop());
        }

        static void Insert(ref PlayerLoopSystem[] arr, int at, Type identifier, PlayerLoopSystem.UpdateFunction action) {
            var k = new PlayerLoopSystem { type = identifier, updateDelegate = action };
            var l = arr.ToList();
            l.Insert(at, k);
            arr = l.ToArray();
        }

        struct ModulesUpdate { }
        struct ModulesLateUpdate { }
        struct ModulesFixedUpdate { }
        struct PostCameraRender { }

    }
}

// static injectors using our own attribute.


// We are GUARANTEED to execute context initialization before any script Awakes are made.
// Lazy Autofilled containers might still be a problem but we now have coded that is guaranteed to execute before their awake.
// Modules container makes more sense now.
//  - no need for an explicit bridge to call / update modules
//  - game modules / contexts should not depend on scene changes.
//  In fact, they frequently CAUSE or MANAGE scene changes and related effects.
// - game modules / contexts can have lifetime, but there exists a thin management layer that is absolutely
// ALWAYS in memory for the duration of app execution.

// explore the potential of ADDITIONAL INTERFACE-BASED CALLBACKS

// definitely explore all the ways in which automated containers now become possible.

// - a name for the ubiquitous layer, which also manages the other stuff. 
// takes the role of Bridge, GameController, StateManager
// - 
// - 

// ExecutionContext
// LogicBag
// Module

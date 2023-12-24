using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace K3 {
    public enum Triggers {
        AppStart,
        Update,
        FixedUpdate,
        LateUpdate,
        PostRender,
        Teardown,
    }
}

namespace K3.Pipeline { 

    public interface IPipeline {

        

        void RegisterMethod(Triggers trigger, Action method, int priority = 0);
    }

    public interface IPipelineInjector {
        void Inject(IPipeline pipeline);
    }

    class CustomPipeline {

        class PipelineInstance : IPipeline {
            public void RegisterMethod(Triggers trigger, Action method, int priority = 0) {
                var mh = new MethodHook { trigger = trigger, priority = priority, method = method };
                CustomPipeline.GetHooks(trigger).Add(mh);
            }
        }

        static IPipelineInjector[] injectors;
        static IPipeline pipelineObject;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static internal void InitializeApplication() {
            RegisterEngineEvents();
            InitializeHookSystem();
            injectors = GetPipelineInjectors().ToArray();
            if (injectors.Length == 0) 
                Debug.LogWarning($"Found ZERO pipeline injectors, this is usually not a good sign");            
            pipelineObject = new PipelineInstance();

            LoopManipulator.ClearEvents();
            LoopManipulator.AddLoopEvents();
            foreach (var injector in injectors) injector.Inject(pipelineObject);
        }

        private static IEnumerable<IPipelineInjector> GetPipelineInjectors() {
            return K3.ReflectionUtility
                .FindImplementingTypesInProject<IPipelineInjector>(true)
                .Select(i => Activator.CreateInstance(i))
                .Cast<IPipelineInjector>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void AppStart() {
            Execute(Triggers.AppStart);
        }

        private static void InitializeHookSystem() {
            hooksPerTrigger = K3.Enums.MapToArrayAndPopulate<List<MethodHook>, Triggers>();
        }

        static List<MethodHook> GetHooks(Triggers forTrigger) => hooksPerTrigger[(int)forTrigger];

        internal static void Execute(Triggers trigger) {
            foreach (var item in GetHooks(trigger)) item.method?.Invoke();
        }

        static void TeardownApplication() {
            Execute(Triggers.Teardown);
            CleanupStatics();
        }

        static void CleanupStatics() {
            Application.quitting -= TeardownApplication;

            foreach (var item in hooksPerTrigger) item.Clear(); // purge hook calls
        }

        private static void RegisterEngineEvents() {
            Application.quitting += TeardownApplication;
        }


        static List<MethodHook>[] hooksPerTrigger;

        public void UnregisterMethod(Action method) {
            foreach (var list in hooksPerTrigger) list.RemoveAll(item => item.method == method);
        }

        struct MethodHook {
            internal Triggers trigger;
            internal int priority;
            internal Action method;
        }
    }
}
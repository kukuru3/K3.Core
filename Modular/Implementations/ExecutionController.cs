using System.Collections.Generic;

namespace K3._ModularOld {
    /// <summary>
    /// Will send Unity callbacks to all services.
    /// Also serves to inject extra callbacks into all actively running Script-s, 
    /// such as Logic - intended to be extended with things like Paused, Unpaused, etc.
    /// </summary>
    public class ExecutionController {
        IGameContext Context { get; set; }

        public void Initialize(IGameContext context) {
            Context = context;
            Context.UnityEventSource.OnFixedUpdate += ProcessFixedUpdate;
            Context.UnityEventSource.OnGUIDrawn += ProcessOnGUIDrawn;
            Context.UnityEventSource.OnLateUpdate += ProcessLateUpdate;
            Context.UnityEventSource.OnUpdate += ProcessUpdate;
            Context.UnityEventSource.OnLogic += ProcessLogic;
            Context.UnityEventSource.OnQuitting += ProcessQuitting;
            Context.UnityEventSource.OnScriptCreated += OnScriptCreated;
            Context.UnityEventSource.OnScriptDestroyed += OnScriptDestroyed;
            Script.isApplicationQuitting = false;
        }

        bool scriptListNeedsCleanup;

        List<Script> activeScripts = new List<Script>();

        private void OnScriptDestroyed(Script obj) {
            scriptListNeedsCleanup = true;
        }

        private void OnScriptCreated(Script obj) {
            activeScripts.Add(obj);
        }

        public IEnumerable<T> ListScripts<T>() {
            foreach (var s in activeScripts) if (s is T st) yield return st;
        }

        internal void Teardown() {
            Context.UnityEventSource.OnFixedUpdate -= ProcessFixedUpdate;
            Context.UnityEventSource.OnGUIDrawn -= ProcessOnGUIDrawn;
            Context.UnityEventSource.OnLateUpdate -= ProcessLateUpdate;
            Context.UnityEventSource.OnUpdate -= ProcessUpdate;
            Context.UnityEventSource.OnLogic -= ProcessLogic;
            Context.UnityEventSource.OnQuitting -= ProcessQuitting;
        }

        private void ProcessLogic() {
            foreach (var m in Context.ModuleContainer.AllModules) {
                if (m is IExecutesLogic iel) iel.Logic();
                if (m is IServiceContainer sp) foreach (var service in sp.Implementing<IExecutesLogic>()) service.Logic();
            }

            if (scriptListNeedsCleanup) MaintainScriptList();
            foreach (var s in activeScripts) s.Logic();
        }

        private void MaintainScriptList() {
            activeScripts.RemoveAll(s => s == null);
            scriptListNeedsCleanup = false;
        }

        private void ProcessUpdate() {
            foreach (var m in Context.ModuleContainer.AllModules) {
                if (m is IExecutesFrame ief) ief.Frame();
                if (m is IServiceContainer sp) foreach (var service in sp.Implementing<IExecutesFrame>()) service.Frame();
            }
        }

        private void ProcessLateUpdate() {
            foreach (var m in Context.ModuleContainer.AllModules) {
                if (m is IExecutesLateUpdate ielu) ielu.LateUpdate();
                if (m is IServiceContainer sp) foreach (var service in sp.Implementing<IExecutesLateUpdate>()) service.LateUpdate();
            }
        }

        private void ProcessOnGUIDrawn() {
            foreach (var m in Context.ModuleContainer.AllModules) {
                if (m is IExecutesGUI iegui) iegui.ExecuteUGUI();
                if (m is IServiceContainer sp) foreach (var service in sp.Implementing<IExecutesGUI>()) service.ExecuteUGUI();
            }
        }

        private void ProcessFixedUpdate() {
            foreach (var m in Context.ModuleContainer.AllModules) {
                if (m is IExecutesTick iet) iet.Tick();
                if (m is IServiceContainer sp) foreach (var service in sp.Implementing<IExecutesTick>()) service.Tick();
            }
        }

        private void ProcessQuitting() {
            Script.isApplicationQuitting = true;
            foreach (var m in Context.ModuleContainer.AllModules) {
                if (m is IExecutesApplicationQuit ieaq) ieaq.ApplicationWillQuit();
                if (m is IServiceContainer sp) foreach (var service in sp.Implementing<IExecutesApplicationQuit>()) service.ApplicationWillQuit();
            }
        }
    }
    
}

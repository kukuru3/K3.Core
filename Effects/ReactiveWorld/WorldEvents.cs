using System;
using System.Collections.Generic;

namespace K3.ReactiveWorld {
    public interface IGameEvent {
        
    }

    public interface IGameEventEmitter {
        event Action<IGameEvent> OnGameEvent;
    }

    public interface IGameEventObserver {
        void Process(IGameEvent evt);
    }

    /// <summary>
    /// The simplest game event relay - all observers are notified about all events sent by all emitters.
    /// Naturally you might want to have some other algorithms and grouping / bucketing.
    /// </summary>
    public class GameEvents : _ModularOld.IExecutesTeardown {
        List<IGameEventEmitter> emitters = new List<IGameEventEmitter>();
        List<IGameEventObserver> observers = new List<IGameEventObserver>();

        public GameEvents() {
            _ModularOld.K3ContextUtilities.Context.UnityEventSource.OnScriptCreated += HandleScriptCreated; 
            _ModularOld.K3ContextUtilities.Context.UnityEventSource.OnScriptDestroyed += HandleScriptDestroyed;
        }

        void _ModularOld.IExecutesTeardown.Teardown() {
            _ModularOld.K3ContextUtilities.Context.UnityEventSource.OnScriptCreated -= HandleScriptCreated;
            _ModularOld.K3ContextUtilities.Context.UnityEventSource.OnScriptDestroyed -= HandleScriptDestroyed;
        }

        void RegisterObserver(IGameEventObserver obs) {
            observers.Add(obs);
        }

        void UnregisterObserver(IGameEventObserver obs) {
            observers.Remove(obs); 
        }
        
        void RegisterEmitter(IGameEventEmitter emitter) {
            emitters.Add(emitter);
            emitter.OnGameEvent += ExecuteGameEvent;
        }

        public void UnregisterEmitter(IGameEventEmitter emt) {
            emt.OnGameEvent -= ExecuteGameEvent;
            emitters.Remove(emt);
        }

        private void HandleScriptCreated(Script obj) {
            if (obj is IGameEventObserver obs) RegisterObserver(obs);
            if (obj is IGameEventEmitter emt) RegisterEmitter(emt);
        }

        private void HandleScriptDestroyed(Script obj) {
            if (obj is IGameEventEmitter emt) UnregisterEmitter(emt);
            if (obj is IGameEventObserver obs) UnregisterObserver(obs);
        }

        public void ExecuteGameEvent(IGameEvent ge) {
            foreach (var observer in observers) observer.Process(ge);
        }

        public void ExecuteGameEvent<T>(T ge) where T : IGameEvent {
            foreach (var observer in observers) observer.Process(ge);
        }
    }
}

namespace K3.Modular {
    class SimpleContext : IGameContext {
        public IAppModuleContainer ModuleContainer { get; internal set; }
        public IHasUnityEvents UnityEventSource { get; internal set; }
        public ExecutionController Executor { get; internal set; }
        public ISceneContainers Containers { get; internal set; }
    }
}

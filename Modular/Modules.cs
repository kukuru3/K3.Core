namespace K3._ModularOld {
    
    internal static class K3ContextUtilities {
        // This is the only static accessor for the context in K3
        // and it is only for internal K3 use.
        // If you want static context access outside K3, you can get one from your launcher.
        internal static IGameContext Context { get; set; }
    }
    
    
    public interface IModularService<TModule> where TModule : IAppModule {
        TModule Module { get; }
    } 
    

    // Application.CreateModule<T>(dependencies)
}
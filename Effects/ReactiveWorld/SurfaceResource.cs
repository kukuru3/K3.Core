using UnityEngine;

namespace K3.ReactiveWorld {
    // Here you can flag surfaces with properties, and then later use
    // those properties for logic or cosmetic effects.

    [CreateAssetMenu(fileName = "surface.asset", menuName = "K3 Gamepak/ReactiveWorld Material")]
    public class SurfaceResource : Resource {
        #pragma warning disable 649
        [SerializeField] string surfaceName;
        [SerializeField][Range(0f, 1f)] float hardness = 0.5f;
        [SerializeField][Range(0f, 1f)] float grip = 0.5f;

        [SerializeField] string[] tags; 
        
        #pragma warning restore 649

        public string Name => surfaceName;
        public float Hardness => hardness;
        public float Grip => grip;

        public string[] Tags => tags;
    }
}

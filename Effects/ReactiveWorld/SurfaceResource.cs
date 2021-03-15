using UnityEngine;

namespace K3.ReactiveWorld {
    // Contains some properties through which you can flag surfaces with properties, and then later use
    // those properties for logic or cosmetic effects.
    // MaterialSurfaceDefinitioniv

    [CreateAssetMenu(fileName = "surface.asset", menuName = "K3 Gamepak/ReactiveWorld Material")]
    public class SurfaceResource : Resource {
        #pragma warning disable 649
        [SerializeField] string surfaceName;
        [SerializeField] bool hasDust;
        [SerializeField] bool isSolid;
        [SerializeField][Range(0f, 1f)] float hardness = 0.5f;
        [SerializeField][Range(0f, 1f)] float grip = 0.5f;
        #pragma warning restore 649

        public string Name => surfaceName;
        public bool Dusty => hasDust;
        public bool Solid => isSolid;
        public float Hardness => hardness;
        public float Grip => grip;
    }
}

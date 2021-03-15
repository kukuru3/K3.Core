using UnityEngine;

namespace K3.Effects {
    [CreateAssetMenu(fileName = "spawnable.asset", menuName = "K3 Gamepak/Spawnable preset")]
    public class Spawnable : Resource {
        #pragma warning disable 649
        [SerializeField] GameObject[] prefabs;
        #pragma warning restore 649

        public GameObject GetNextPrefab() => prefabs.PickRandom();
    }
}

using UnityEngine;

namespace K3 {
    public class Resource : ScriptableObject {
        public string ResourceID => name;
    }

    public class ResourceReference<T> where T : Resource {
        public ResourceReference(string id) {
            this.resourceID = id;
        }
        public readonly string resourceID;
        public T GetResource() {
            var l = Resources.Load(resourceID);
            return (T)l;
        }
    }
}

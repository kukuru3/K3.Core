using UnityEngine;
using System.IO;

namespace K3.Seralization {

    public static class SerializationUtility {
        public static void Write(this BinaryWriter bw, Vector3 vector) {
            bw.Write(vector.x);
            bw.Write(vector.y);
            bw.Write(vector.z);
        }

        public static Vector3 ReadVector3(this BinaryReader bw) {
            var x = bw.ReadSingle();
            var y = bw.ReadSingle();
            var z = bw.ReadSingle();
            return new Vector3(x,y,z);
        }
    }

}
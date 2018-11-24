using System;
using UnityEngine;

namespace Game.Utility {
    [Serializable]
    public struct NVector3 {
        public float x;
        public float y;
        public float z;

        public NVector3(float x, float y, float z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public NVector3(Vector3 vector) {
            this.x = vector.x;
            this.y = vector.y;
            this.z = vector.z;
        }

        public Vector3 ToV() {
            return new Vector3(this.x, this.y, this.z);
        }
    }

    public static class Math {
        public static NVector3 ToN(this Vector3 vector) {
            return new NVector3(vector);
        }
    }
}
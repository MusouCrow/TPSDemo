using System;
using System.Text;
using UnityEngine;

namespace Game.Utility {
    public static class Math {
        public static float ToFixed(this float value) {
            return Mathf.Floor(Mathf.Abs(value * 1000)) * 0.001f * value.ToDirection();
        }

        public static int ToDirection(this float value) {
            return value >= 0 ? 1 : -1;
        }

        public static Vector3 ToFixed(this Vector3 value) {
            value.x = value.x.ToFixed();
            value.y = value.y.ToFixed();
            value.z = value.z.ToFixed();

            return value;
        }
        
        public static string BytesToStr(byte[] bytes) {
            var sb = new StringBuilder(); 

            for (int i = 0; i < bytes.Length; i++) {
                sb.Append(bytes[i]);
            }

            return sb.ToString();
        }

        public static string ToBinStr(this float value) {
            return BytesToStr(BitConverter.GetBytes(value));
        }

        public static float Lerp(float a, float b, float t) {
            return Mathf.Lerp(a.ToFixed(), b.ToFixed(), t.ToFixed()).ToFixed();
        }

        public static float Random() {
            var value = UnityEngine.Random.value.ToFixed();
            //Debug.LogError(value);
            return value;
        }
    }
}
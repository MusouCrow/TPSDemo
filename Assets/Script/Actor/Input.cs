using System;
using UnityEngine;

namespace Game.Actor {
    public enum KeyStatus {Free, Pressed, Hold, Released}

    public class Input : MonoBehaviour {
        [NonSerialized]public float vertical;
        [NonSerialized]public float horizontal;
        [NonSerialized]public float mouseX;
        [NonSerialized]public KeyStatus fire;

        protected void LateUpdate() {
            this.UpdateKey(ref this.fire);
        }

        public void SetData(Network.InputData data) {
            this.vertical = data.vertical;
            this.horizontal = data.horizontal;
            this.mouseX = data.mouseX;
            this.HandleKey(ref this.fire, data.fire);
        }

        private void HandleKey(ref KeyStatus key, bool value) {
            if (value && key != KeyStatus.Hold) {
                key = KeyStatus.Pressed;
            }
            else if (!value && key != KeyStatus.Free) {
                key = KeyStatus.Released;
            }
        }

        private void UpdateKey(ref KeyStatus key) {
            if (key == KeyStatus.Pressed) {
                key = KeyStatus.Hold;
            }
            else if (key == KeyStatus.Released) {
                key = KeyStatus.Free;
            } 
        }
    }
}

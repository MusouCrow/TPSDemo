using System;
using UnityEngine;

namespace Game.Shooter {
    public class Fire : Network.LockBehaviour {
        public float speed;
        public GameObject bullet;
        
        private Input input;

        protected void Start() {
            this.input = this.GetComponent<Input>();
        }

        protected override void LockUpdate() {
            float value = this.input.mouseX;
            
            if (value != 0) {
                this.transform.Rotate(0, value * speed, 0);
            }

            if (this.input.fire == KeyStatus.Pressed) {
                GameObject.Instantiate(this.bullet, this.transform.position, this.transform.rotation);
            } 
        }
    }
}

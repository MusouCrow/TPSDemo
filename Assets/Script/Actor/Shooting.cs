using System;
using UnityEngine;

namespace Game.Actor {
    public class Shooting : Network.LockBehaviour {
        public float power;
        public float speed;
        
        protected override void LockUpdate() {
            this.transform.Translate(this.power, 0, 0);
            this.power -= this.speed;

            if (this.power <= 0) {
                GameObject.Destroy(this.gameObject);
            }
        }
    }
}
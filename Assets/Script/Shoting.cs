using System;
using UnityEngine;

namespace Game {
    public class Shoting : MonoBehaviour {
        public float power;
        public float speed;
        
        protected void FixedUpdate() {
            this.transform.Translate(this.power, 0, 0);
            this.power -= this.speed;

            if (this.power <= 0) {
                GameObject.Destroy(this.gameObject);
            }
        }
    }
}

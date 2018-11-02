using System;
using UnityEngine;

namespace Game.Shooter {
    public class Moving : Network.LockBehaviour {
        public float speed;

        private Input input;

        protected void Start() {
            this.input = this.GetComponent<Input>();
        }

        protected override void LockUpdate() {
            if (this.input.vertical != 0 || this.input.horizontal != 0) {
                this.transform.Translate(this.speed * this.input.vertical, 0, this.speed * -this.input.horizontal);
            }
        }
    }
}

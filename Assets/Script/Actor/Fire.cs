using System;
using UnityEngine;

namespace Game.Actor {
    using Network;

    public class Fire : MonoBehaviour {
        private Identity identity;
        private Vector3 velocity; 

        protected void Start() {
            this.identity = this.GetComponent<Identity>();
            this.identity.BindEvent(typeof(Snapshots.Rotate), this.Rotate);
        }

        protected void FixedUpdate() {
            if (this.identity.IsPlayer) {
                var value = Input.GetAxis("Mouse X");

                if (Mathf.Abs(value) > 0.5f) {
                    var rotate = new Snapshots.Rotate() {
                        velocity = new Vector3(0, value, 0),
                        rotation = this.transform.rotation
                    };
                    this.identity.Input(rotate);
                }
            }
        }

        public void Simulate() {
            if (this.velocity != Vector3.zero) {
                this.transform.Rotate(this.velocity);
                this.velocity = Vector3.zero;
            }
        }

        private void Rotate(Snapshot snapshot) {
            var rotate = snapshot as Snapshots.Rotate;
            
            if (ServerMgr.Active) {
                rotate.rotation = this.transform.rotation;
            }
            else {
                this.transform.rotation = rotate.rotation;
            }

            this.velocity = rotate.velocity;
        }
    }
}
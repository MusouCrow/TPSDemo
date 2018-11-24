using System;
using UnityEngine;

namespace Game.Actor {
    public class Motion : MonoBehaviour {
        private Identity identity;
        private Vector3 velocity;

        protected void Start() {
            this.identity = this.GetComponent<Identity>();
            this.identity.BindEvent(typeof(Snapshots.Move), this.Move);
        }

        protected void FixedUpdate() {
            var x = Input.GetAxis("Vertical") * 0.5f;
            var z = -Input.GetAxis("Horizontal") * 0.5f;

            if (x != 0 || z != 0) {
                var move = new Snapshots.Move() {
                    velocity = new Vector3(x, 0, z),
                    position = this.transform.position
                };
                this.identity.Input(move);
            }
        }

        public void Simulate() {
            if (this.velocity != Vector3.zero) {
                this.transform.Translate(this.velocity);
                //this.velocity = Vector3.zero;
            }
        }

        private void Move(Snapshot snapshot) {
            var move = snapshot as Snapshots.Move;
            //this.transform.position = move.position;
            this.velocity = move.velocity;
        }
    }
}

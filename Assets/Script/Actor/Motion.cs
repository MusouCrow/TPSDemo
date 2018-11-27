using System;
using UnityEngine;

namespace Game.Actor {
    using Network;

    public class Motion : MonoBehaviour {
        private Identity identity;
        private Vector3 velocity;
        private Vector3 later;

        protected void Start() {
            this.identity = this.GetComponent<Identity>();
            this.identity.BindEvent(typeof(Snapshots.Move), this.Move);
        }

        protected void FixedUpdate() {
            if (this.identity.IsPlayer) {
                var x = Input.GetAxis("Vertical") * 0.5f;
                var z = -Input.GetAxis("Horizontal") * 0.5f;

                if (x != 0 || z != 0 || this.later != Vector3.zero) {
                    this.later = new Vector3(x, 0, z);
                    var move = new Snapshots.Move() {
                        velocity = this.later,
                        position = this.transform.position
                    };
                    this.identity.Input(move);
                }

                if (Input.GetKeyDown(KeyCode.T)) {
                    this.transform.position = new Vector3(0, 10, 0);
                }
            }
        }

        public void Simulate() {
            if (this.velocity != Vector3.zero) {
                this.transform.Translate(this.velocity);
            }
        }

        private void Move(Snapshot snapshot) {
            var move = snapshot as Snapshots.Move;
            
            if (ServerMgr.Active) {
                move.position = this.transform.position;
            }
            else {
                this.transform.position = move.position;
            }

            //print(Time.frameCount + "," + move.position);
            this.velocity = move.velocity;
        }
    }
}

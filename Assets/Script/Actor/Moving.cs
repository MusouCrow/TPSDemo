using System;
using UnityEngine;

namespace Game.Actor {
    using Network;

    public class Moving : MonoBehaviour {
        private static float gravity = -0.05f;

        private Identity identity;
        private CharacterController controller;
        private Shaking shaking;
        private Vector3 velocity;
        private float velocityY;

        protected void Start() {
            this.identity = this.GetComponent<Identity>();
            this.identity.BindEvent(typeof(Snapshots.Move), this.Move);
            this.controller = this.GetComponent<CharacterController>();
            this.shaking = this.GetComponent<Shaking>();
        }

        protected void FixedUpdate() {
            if (this.identity.IsPlayer) {
                var x = Input.GetAxis("Vertical") * 0.5f;
                var y = this.velocityY;
                var z = -Input.GetAxis("Horizontal") * 0.5f;

                if (this.controller.isGrounded && Input.GetKeyDown(KeyCode.Space)) {
                    y = 0.6f;
                    this.shaking.Shake(0.1f, 0.05f);
                }

                if (!this.controller.isGrounded) {
                    y += Moving.gravity;
                }

                if (x != 0 || y != this.velocityY || z != 0) {
                    var move = new Snapshots.Move() {
                        velocity = new Vector3(x, y, z),
                        position = this.transform.position
                    };
                    this.identity.Input(move);
                }

                this.velocityY = y;
            }
        }

        public void Simulate() {
            if (this.velocity != Vector3.zero) {
                this.controller.Move(this.transform.TransformDirection(this.velocity));
                this.velocity = Vector3.zero;
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
            
            this.velocity = move.velocity;
        }
    }
}

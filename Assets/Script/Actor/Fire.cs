using System;
using UnityEngine;

namespace Game.Actor {
    using Network;

    public class Fire : MonoBehaviour {
        public GameObject bullet;

        private Identity identity;
        private Vector3 velocity; 
        private int shootingCount;

        protected void Start() {
            this.identity = this.GetComponent<Identity>();
            this.identity.BindEvent(typeof(Snapshots.Rotate), this.Rotate);
            this.identity.BindEvent(typeof(Snapshots.Shoot), this.Shoot);
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

                if (Input.GetKeyDown(KeyCode.Mouse0)) {
                    var shoot = new Snapshots.Shoot() {
                        position = this.transform.position,
                        rotation = this.transform.rotation
                    };
                    this.identity.Input(shoot);
                }
            }
        }

        public void Simulate() {
            if (this.velocity != Vector3.zero) {
                this.transform.Rotate(this.velocity);
                this.velocity = Vector3.zero;
            }

            if (this.shootingCount > 0) {
                GameObject.Instantiate(this.bullet, this.transform.position, this.transform.rotation);
                this.shootingCount--;
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

        private void Shoot(Snapshot snapshot) {
            var shoot = snapshot as Snapshots.Shoot;

            if (ServerMgr.Active) {
                shoot.position = this.transform.position;
                shoot.rotation = this.transform.rotation;
            }
            else {
                this.transform.position = shoot.position;
                this.transform.rotation = shoot.rotation;
            }

            this.shootingCount++;
        }
    }
}
using System;
using UnityEngine;

namespace Game.Actor {
    using Network;

    public class Shooting : MonoBehaviour {
        public GameObject bullet;
        public AudioClip clip;

        private Identity identity;
        private Shaking shaking;
        private Vector3 velocity; 
        private int shootingCount;

        protected void Start() {
            this.identity = this.GetComponent<Identity>();
            this.identity.BindEvent(typeof(Snapshots.Rotate), this.Rotate);
            this.identity.BindEvent(typeof(Snapshots.Shoot), this.Shoot);

            this.shaking = this.GetComponent<Shaking>();
        }

        protected void FixedUpdate() {
            if (this.identity.IsPlayer) {
                var value = Input.GetAxis("Mouse X");
                //print("FixedUpdate: " + Time.frameCount + ", " + Time.time);

                if (Mathf.Abs(value) > 0) {
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
            //print("Simulate: " + Time.frameCount + ", " + Time.time);

            if (this.velocity != Vector3.zero) {
                this.transform.Rotate(this.velocity);
                this.velocity = Vector3.zero;
            }

            if (this.shootingCount > 0) {
                GameObject.Instantiate(this.bullet, this.transform.position + this.transform.TransformDirection(Vector3.right), this.transform.rotation);
                this.shootingCount--;
                this.shaking.Shake(0.3f, 0.05f);
                AudioSource.PlayClipAtPoint(this.clip, this.transform.position);
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
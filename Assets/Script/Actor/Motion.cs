using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Game.Actor {
    using Utility;

    public class Motion : MonoBehaviour {
        private Identity identity;
        private Vector3 velocity;

        protected void Start() {
            this.identity = this.GetComponent<Identity>();
            this.identity.BindEvent("Move", this.Move);
        }

        protected void FixedUpdate() {
            var x = Input.GetAxis("Vertical") * 0.5f;
            var z = -Input.GetAxis("Horizontal") * 0.5f;

            var move = new Snapshots.Move() {
                velocity = new NVector3(x, 0, z),
                position = this.transform.position.ToN()
            };
            this.identity.Input("Move", move);
        }

        public void Simulate() {
            if (this.velocity != Vector3.zero) {
                this.transform.Translate(this.velocity);
                this.velocity = Vector3.zero;
            }
        }

        private void Move(Snapshot snapshot) {
            var move = JsonConvert.DeserializeObject<Snapshots.Move>(snapshot.obj.ToString());
            //this.transform.position = move.position;
            this.velocity = move.velocity.ToV();
        }
    }
}

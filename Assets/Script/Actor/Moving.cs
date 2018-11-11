using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Game.Actor {
    using Network;
    using Datas = Network.Datas;

    public class Moving : Network.LockBehaviour {
        private Identity identity;
        private SpriteRenderer spriteRenderer;
        private Vector3 velocity;
        private int count;

        protected void Start() {
            this.identity = this.GetComponent<Identity>();
            this.spriteRenderer = this.GetComponent<SpriteRenderer>();

            this.identity.AddEvent("Move", this.Move);
        }
        
        protected override void LockUpdate() {
            if (this.identity.IsPlayer) {
                var x = Input.GetAxis("Vertical");
                var z = -Input.GetAxis("Horizontal");

                if (this.velocity.x != x || this.velocity.z != z) {
                    this.identity.Send("Move", new Datas.Move() {x = x, z = z});
                }
            }

            if (this.velocity != Vector3.zero) {
                this.transform.Translate(this.velocity);
            }
        }

        private void Move(object data) {
            var obj = NetworkMgr.DataToObject<Datas.Move>(data);
            this.velocity = new Vector3(obj.x, 0, obj.z);
        }
    }
}

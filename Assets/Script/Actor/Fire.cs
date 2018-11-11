using System;
using UnityEngine;

namespace Game.Actor {
    using Network;
    using Datas = Network.Datas;

    public class Fire : Network.LockBehaviour {
        public GameObject bullet;
        
        private Identity identity;
        private float rotatingValue;

        protected void Start() {
            this.identity = this.GetComponent<Identity>();

            this.identity.AddEvent("Rotate", this.Rotate);
            this.identity.AddEvent("Fire", this.Shoot);
        }

        protected override void LockUpdate() {
            if (this.identity.IsPlayer) {
                var value = Input.GetAxis("Mouse X");
                
                if (this.rotatingValue != value) {
                    this.identity.Send("Rotate", new Datas.Rotate() {value = value});
                }

                if (Input.GetKeyDown(KeyCode.Mouse0)) {
                    this.identity.Send("Fire", null);
                }
            }

            if (this.rotatingValue != 0) {
                this.transform.Rotate(0, this.rotatingValue, 0);
            }
        }

        private void Rotate(object data) {
            var obj = NetworkMgr.DataToObject<Datas.Rotate>(data);
            this.rotatingValue = obj.value;
        }

        private void Shoot(object data) {
            GameObject.Instantiate(this.bullet, this.transform.position, this.transform.rotation);
        }
    }
}
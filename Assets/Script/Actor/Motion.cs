using System;
using UnityEngine;

namespace Game.Actor {
    using Network;

    public class Motion : MonoBehaviour {
        private new Renderer renderer;
        private Identity identity;

        protected void Start() {
            this.renderer = this.GetComponent<Renderer>();    

            this.identity = this.GetComponent<Identity>();
            this.identity.BindEvent(typeof(Snapshots.ChangeColor), this.ChangeColor);
        }

        protected void FixedUpdate() {
            if (this.identity.IsPlayer && Input.GetKeyDown(KeyCode.Space)) {
                var snapshot = new Snapshots.ChangeColor() {
                    isWhite = this.renderer.material.color == Color.white
                };
                this.identity.Input(snapshot);
                //print(Client.FrameCount);
            }
        }

        private void ChangeColor(Snapshot snapshot) {
            var changeColor = snapshot as Snapshots.ChangeColor;
            this.renderer.material.color = changeColor.isWhite ? Color.black : Color.white;
            //print(Client.FrameCount);
        }
    }
}
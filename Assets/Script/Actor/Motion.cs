using System;
using UnityEngine;

namespace Game.Actor {
    using Network;

    public class Motion : MonoBehaviour {
        private new Renderer renderer;

        protected void Start() {
            this.renderer = this.GetComponent<Renderer>();    

            var identity = this.GetComponent<Identity>();
            identity.BindEvent(typeof(Snapshots.ChangeColor), this.ChangeColor);
        }

        protected void FixedUpdate() {
            if (Input.GetKeyDown(KeyCode.Space)) {
                var color = this.renderer.material.color == Color.white ? Color.black : Color.white;
                var snapshot = new Snapshots.ChangeColor();
                Client.Input(snapshot);
                print(Client.FrameCount);
            }
        }

        private void ChangeColor(Snapshot snapshot) {
            var color = snapshot as Snapshots.ChangeColor;
            this.renderer.material.color = this.renderer.material.color == Color.white ? Color.black : Color.white;
            print(Client.FrameCount);
        }
    }
}
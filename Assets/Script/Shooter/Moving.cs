using System;
using UnityEngine;

namespace Game.Shooter {
    public class Moving : MonoBehaviour {
        public float speed;

        protected void FixedUpdate() {
            Vector2Int axis = new Vector2Int();
            
            if (Input.GetKey(KeyCode.A)) {
                axis.x = -1;
            }
            else if (Input.GetKey(KeyCode.D)) {
                axis.x = 1;
            }

            if (Input.GetKey(KeyCode.W)) {
                axis.y = 1;
            }
            else if (Input.GetKey(KeyCode.S)) {
                axis.y = -1;
            }

            if (axis.x != 0 || axis.y != 0) {
                this.transform.Translate(this.speed * axis.x, 0, this.speed * axis.y);
            }            
        }
    }
}

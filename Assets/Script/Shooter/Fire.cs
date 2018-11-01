using System;
using UnityEngine;

namespace Game.Shooter {
    public class Fire : MonoBehaviour {
        public float speed;
        public GameObject bullet;

        protected void FixedUpdate() {
            float value = Input.GetAxis("Mouse X");
            
            if (value != 0) {
                this.transform.Rotate(0, value * speed, 0);
            }

            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                GameObject.Instantiate(this.bullet, this.transform.position, this.transform.rotation);
            } 
        }
    }
}

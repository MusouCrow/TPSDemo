using System;
using UnityEngine;

namespace Game.Actor {
    public class Bullet : MonoBehaviour {
        public float power;
        public float speed;
        public GameObject effect;

        protected void FixedUpdate() {
            var origin = this.transform.position;
            this.transform.Translate(this.power, 0, 0);
            this.power -= this.speed;
            float length = (transform.position - origin).magnitude;
            Vector3 direction = transform.position - origin;
            bool isCollided = Physics.Raycast(this.transform.position, direction, length);

            if (this.power <= 0 || isCollided) {
                GameObject.Instantiate(this.effect, this.transform.position, this.transform.rotation);
                GameObject.Destroy(this.gameObject);
            }
        }
    }
}
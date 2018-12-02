using System;
using UnityEngine;

namespace Game.Actor {
    public class Bullet : MonoBehaviour {
        public float power;
        public float speed;
        public GameObject effect;
        public AudioClip clip;

        private int direction = 1;

        protected void FixedUpdate() {
            RaycastHit hit;
            var ts = this.transform;
            var origin = ts.position;
            ts.Translate(this.power * this.direction, 0, 0);
            this.power -= this.speed;
            float length = (transform.position - origin).magnitude;
            Vector3 direction = transform.position - origin;
            bool isCollided = Physics.Raycast(ts.position, direction, out hit, length);

            if (isCollided) {
                GameObject.Instantiate(this.effect, hit.point, ts.rotation);
                this.direction = -this.direction;
                ts.rotation = Quaternion.Euler(ts.rotation.eulerAngles * 1.2f);
                AudioSource.PlayClipAtPoint(this.clip, ts.position);

                var battle = hit.transform.gameObject.GetComponent<Battle>();

                if (battle != null) {
                    battle.Beaten();
                }
            }
            
            if (this.power <= 0) {
                GameObject.Destroy(this.gameObject);
            }
        }
    }
}
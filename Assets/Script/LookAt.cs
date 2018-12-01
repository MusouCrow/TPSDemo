using UnityEngine;

namespace Game {
    public class LookAt : MonoBehaviour {
        private Transform target;

        protected void Start() {
            this.target = Camera.main.transform;
        }

        protected void FixedUpdate() {
            this.transform.rotation = this.target.rotation;
        }
    }
}
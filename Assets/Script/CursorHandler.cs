using UnityEngine;

namespace Game {
    public class CursorHandler : MonoBehaviour {
        protected void Awake() {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
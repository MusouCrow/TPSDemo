using System;
using UnityEngine;

namespace Game.Actor {
    public class CursorHandler : MonoBehaviour {
        protected void Awake() {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
using System;
using UnityEngine;

namespace Game.Actor {
    public class Identity : MonoBehaviour {
        [NonSerialized]
        public int connectionId;

        public PlayerData ToPlayerData() {
            return new PlayerData() {
                connectionId = this.connectionId,
                position = this.transform.position
            };
        }
    }
}

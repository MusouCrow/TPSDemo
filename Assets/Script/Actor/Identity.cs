using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Actor {
    public class Identity : MonoBehaviour {
        [NonSerialized]
        public int connectionId;
        private Dictionary<Type, Action<Snapshot>> eventMap;

        protected void Awake() {
            this.eventMap = new Dictionary<Type, Action<Snapshot>>();
        }

        public void BindEvent(Type type, Action<Snapshot> Func) {
            this.eventMap.Add(type, Func);
        }

        public void RunEvent(Snapshot snapshot) {
            if (this.eventMap.ContainsKey(snapshot.GetType())) {
                this.eventMap[snapshot.GetType()](snapshot);
            }
        }

        public PlayerData ToPlayerData() {
            return new PlayerData() {
                connectionId = this.connectionId,
                position = this.transform.position
            };
        }
    }
}

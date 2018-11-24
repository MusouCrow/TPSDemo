using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Actor {
    using Network;
    using Utility;

    public class Identity : MonoBehaviour {
        [NonSerialized]
        public string fd;
        private Dictionary<string, Action<Snapshot>> eventMap;

        public bool IsPlayer {
            get {
                return this.fd == ClientMgr.FD;
            }
        }

        protected void Awake() {
            this.eventMap = new Dictionary<string, Action<Snapshot>>();
        }

        public void BindEvent(string type, Action<Snapshot> Func) {
            this.eventMap.Add(type, Func);
        }

        public void RunEvent(Snapshot snapshot) {
            var type = snapshot.type;
            
            if (this.eventMap.ContainsKey(type)) {
                this.eventMap[type](snapshot);
            }
        }

        public PlayerData ToPlayerData() {
            return new PlayerData() {
                fd = this.fd,
                position = this.transform.position.ToN()
            };
        }

        public void Input(string type, object obj) {
            //this.RunEvent(snapshot);
            ClientMgr.Input(type, obj);
        }
    }
}

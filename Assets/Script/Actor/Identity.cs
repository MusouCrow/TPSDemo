using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Actor {
    using Network;

    public class Identity : MonoBehaviour {
        [NonSerialized]
        public string fd;
        private Dictionary<Type, Action<Snapshot>> eventMap;

        public bool IsPlayer {
            get {
                return this.fd == ClientMgr.FD;
            }
        }

        protected void Awake() {
            this.eventMap = new Dictionary<Type, Action<Snapshot>>();
        }

        public void BindEvent(Type type, Action<Snapshot> Func) {
            this.eventMap.Add(type, Func);
        }

        public void RunEvent(Snapshot snapshot) {
            var type = snapshot.GetType();
            
            if (this.eventMap.ContainsKey(type)) {
                this.eventMap[type](snapshot);
            }
        }
        /*
        public PlayerData ToPlayerData() {
            return new PlayerData() {
                fd = this.fd,
                position = this.transform.position
            };
        } */

        public void HandlePlayerData(PlayerData playerData) {
            playerData.fd = this.fd;
            playerData.position = this.transform.position;
            playerData.rotation = this.transform.rotation;
        }

        public void Input(Snapshot snapshot) {
            if (!ServerMgr.Active) {
                this.RunEvent(snapshot);
            }
            
            ClientMgr.Input(snapshot);
        }

        public void ServerInput(Snapshot snapshot) {
            if (!ServerMgr.Active) {
                return;
            }

            if (ServerMgr.IsPlayer) {
                this.RunEvent(snapshot);
            }

            ServerMgr.Input(this.fd, snapshot);
        }
    }
}

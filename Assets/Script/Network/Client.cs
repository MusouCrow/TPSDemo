using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Network {
    using Actor;
    using Snapshots = Actor.Snapshots;

    public class Client : MonoBehaviour {
        private static Client INSTANCE;
        private const int INTERVAL = 5;

        public static void Input(Snapshot snapshot) {
            snapshot.frame = Client.FrameCount;
            INSTANCE.sendList.Add(snapshot);

            if (!NetworkServer.active) {
                INSTANCE.checkList.Add(snapshot);
            }
        }

        public static List<List<Snapshot>> SyncList {
            get {
                return INSTANCE.syncList;
            }
        }

        public static bool IsPlayer {
            get {
                return INSTANCE.isPlayer;
            }
        }

        public static int FrameCount {
            get {
                return INSTANCE.frameCount;
            }
        }

        public static int ConnectionId {
            get {
                return INSTANCE.connectionId;
            }
        }

        private static void Resolve(GameObject gameObject, List<Snapshot> list, int index) {
            for (int i = index; i < list.Count; i++) {
                list[i].Resolve(gameObject);
                
                if (i < list.Count - 1 && list[i].frame != list[i + 1].frame) {
                    gameObject.SendMessage("FixedUpdate");
                }
            }

            gameObject.SendMessage("FixedUpdate");
        }

        [SerializeField]
        private string address;
        [SerializeField]
        private int port;
        [SerializeField]
        private bool isPlayer;
        private NetworkClient client;
        private List<Snapshot> sendList;
        private List<Snapshot> checkList;
        private List<List<Snapshot>> syncList;
        private int frameCount;
        private int connectionId;

        protected void Start() {
            INSTANCE = this;

            this.sendList = new List<Snapshot>();
            this.checkList = new List<Snapshot>();
            this.syncList = new List<List<Snapshot>>();

            this.client = new NetworkClient();
            this.client.RegisterHandler(MsgType.Connect, this.OnConnected);
            this.client.RegisterHandler(MsgType.Disconnect, this.OnDisconnected);
            this.client.RegisterHandler(MsgTypes.NewPlayer, this.NewPlayer);
            this.client.RegisterHandler(MsgTypes.DelPlayer, this.DelPlayer);
            this.client.RegisterHandler(MsgTypes.Start, this.Start);
            this.client.RegisterHandler(MsgTypes.Sync, this.Sync);
            this.client.Connect(this.address, this.port);
        }

        protected void FixedUpdate() {
            if (!this.client.isConnected) {
                return;
            }
            
            if (this.sendList.Count == 0 || this.sendList[this.sendList.Count - 1].frame != this.frameCount) {
                Client.Input(new Snapshot());
            }
            
            this.frameCount++;

            if (this.syncList.Count > 0) {
                foreach (var s in this.syncList[0]) {
                    if (s.connectionId != this.connectionId) {
                        ActorMgr.Input(s);
                    }
                }
                this.syncList.RemoveAt(0);
                //print(this.syncList.Count);
            }

            if (this.frameCount % Client.INTERVAL == 0) {
                var msg = new Msgs.Input() {
                    snapshotList = this.sendList
                };
                this.client.Send(MsgTypes.Input, msg);
                this.sendList.Clear();
            }
        }

        private void OnConnected(NetworkMessage netMsg) {
            print("Connected");
        }

        private void OnDisconnected(NetworkMessage netMsg) {
            print("Disconnected");
        }

        private void NewPlayer(NetworkMessage netMsg) {
            var msg = new Msgs.NewPlayer();
            msg.Deserialize(netMsg.reader);

            ActorMgr.NewPlayer(msg.connectionId, msg.position, this.client.connection.connectionId == msg.connectionId);
        }

        private void DelPlayer(NetworkMessage netMsg) {
            var msg = new Msgs.DelPlayer();
            msg.Deserialize(netMsg.reader);

            ActorMgr.DelPlayer(msg.connectionId);
        }

        private void Start(NetworkMessage netMsg) {
            var msg = new Msgs.Start();
            msg.Deserialize(netMsg.reader);

            this.connectionId = msg.connectionId;

            foreach (var p in msg.playerDatas) {
                ActorMgr.NewPlayer(p.connectionId, p.position, false);
            }
        }

        private void Sync(NetworkMessage netMsg) {
            if (NetworkServer.active) {
                return;
            }

            var msg = new Msgs.Sync();
            msg.snapshotsList = this.syncList;
            msg.Deserialize(netMsg.reader);

            var list = new List<Snapshot>();

            foreach (var sl in this.syncList) {
                bool hasAdded = false;

                foreach (var s in sl) {
                    if (this.connectionId == s.connectionId) {
                        list.Add(s);
                        hasAdded = true;
                    }
                    else if (hasAdded) {
                        break;
                    }
                }
            }
            
            int index = list.Count;

            for (int i = 0; i < list.Count; i++) {
                if (!list[i].Equals(this.checkList[i])) {
                    index = i;
                    print(i);
                    break;
                }
            }

            this.checkList.RemoveRange(0, list.Count);

            /*
            if (index == list.Count) {
                this.checkList.Clear();
            }
            else {
                var player = ActorMgr.GetPlayer(this.connectionId);
                var frame = list[list.Count - 1].frame;

                for (int i = this.checkList.Count - 1; i >= 0; i--) {
                    if (this.checkList[i].frame <= frame) {
                        this.checkList.RemoveAt(i);
                    }
                }

                Client.Resolve(player, list, index);
                Client.Resolve(player, this.checkList, 0);
            } */
        }
    }
}

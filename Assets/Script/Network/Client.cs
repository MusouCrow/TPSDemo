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

            foreach (var sl in this.syncList) {
                
            }
        }
    }
}

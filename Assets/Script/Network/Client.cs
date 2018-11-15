using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Network {
    using Actor;
    using Snapshots = Actor.Snapshots;

    public class Client : MonoBehaviour {
        private static Client INSTANCE;

        public static void Input(Snapshot snapshot) {
            INSTANCE.snapshotList.Add(snapshot);
        }

        [SerializeField]
        private string address;
        [SerializeField]
        private int port;
        private NetworkClient client;
        private List<Snapshot> snapshotList;
        private int frameCount;

        protected void Start() {
            INSTANCE = this;

            this.snapshotList = new List<Snapshot>();

            this.client = new NetworkClient();
            this.client.RegisterHandler(MsgType.Connect, this.OnConnected);
            this.client.RegisterHandler(MsgType.Disconnect, this.OnDisconnected);
            this.client.RegisterHandler(MsgTypes.NewPlayer, this.NewPlayer);
            this.client.RegisterHandler(MsgTypes.DelPlayer, this.DelPlayer);
            this.client.RegisterHandler(MsgTypes.Start, this.Start);
            this.client.Connect(this.address, this.port);
        }

        protected void FixedUpdate() {
            this.frameCount++;

            if (UnityEngine.Input.GetKeyDown(KeyCode.Space)) {
                var snapshot = new Snapshots.Test() {
                    frame = this.frameCount,
                    content = "测试一下"
                };
                Client.Input(snapshot);
            }

            if (this.frameCount % 5 == 0 && this.snapshotList.Count > 0) {
                var msg = new Msgs.Input() {
                    snapshotList = this.snapshotList
                };
                this.client.Send(MsgTypes.Input, msg);
                this.snapshotList.Clear();
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

            foreach (var p in msg.playerDatas) {
                ActorMgr.NewPlayer(p.connectionId, p.position, false);
            }
        }
    }
}

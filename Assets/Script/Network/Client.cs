using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Network {
    using Actor;

    public class Client : MonoBehaviour {
        [SerializeField]
        private string address;
        [SerializeField]
        private int port;
        private NetworkClient client;

        protected void Start() {
            this.client = new NetworkClient();

            this.client.RegisterHandler(MsgType.Connect, this.OnConnected);
            this.client.RegisterHandler(MsgType.Disconnect, this.OnDisconnected);
            this.client.RegisterHandler(MsgTypes.NewPlayer, this.NewPlayer);
            this.client.RegisterHandler(MsgTypes.DelPlayer, this.DelPlayer);
            this.client.RegisterHandler(MsgTypes.Start, this.Start);

            this.client.Connect(this.address, this.port);
        }
        
        protected void FixedUpdate() {
            if (Input.GetKeyDown(KeyCode.Space)) {
                var msg = new Msgs.Test();
                msg.content = "测试一下";
                this.client.Send(MsgTypes.Test, msg);
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

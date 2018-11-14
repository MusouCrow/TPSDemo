using System;
using UnityEngine;
using UnityEngine.Networking;

using Random = UnityEngine.Random;

namespace Game.Network {
    using Actor;
    using Snapshots = Actor.Snapshots;

    public class Server : MonoBehaviour {
        [SerializeField]
        private int port;
        [SerializeField]
        private bool isPlayer;

        protected void Start() {
            bool ret = NetworkServer.Listen(this.port);

            if (ret) {
                NetworkServer.RegisterHandler(MsgType.Connect, this.OnClientConnected);
                NetworkServer.RegisterHandler(MsgType.Disconnect, this.OnClientDisconnected);
                NetworkServer.RegisterHandler(MsgTypes.Test, this.Test);
                NetworkServer.RegisterHandler(MsgTypes.Sync, this.Sync);

                print("Server Start");
            }
            else {
                Destroy(this);
            }
        }

        private void OnClientConnected(NetworkMessage netMsg) {
            if (!this.isPlayer && NetworkServer.connections[1] == netMsg.conn) {
                return;
            }

            {
                var msg = new Msgs.Start();
                msg.playerDatas = ActorMgr.ToPlayerDatas();

                NetworkServer.SendToClient(netMsg.conn.connectionId, MsgTypes.Start, msg);
            }

            {
                var x = Mathf.Lerp(-2, 2, Random.value);
                var z = Mathf.Lerp(-2, 2, Random.value);

                var msg = new Msgs.NewPlayer();
                msg.connectionId = netMsg.conn.connectionId;
                msg.position = new Vector3(x, 0, z);

                NetworkServer.SendToAll(MsgTypes.NewPlayer, msg);
            }

            print("Client Connected: " + netMsg.conn.connectionId);
        }

        private void OnClientDisconnected(NetworkMessage netMsg) {
            var msg = new Msgs.DelPlayer();
            msg.connectionId = netMsg.conn.connectionId;

            NetworkServer.SendToAll(MsgTypes.DelPlayer, msg);
            print("Client Disconnected: " + netMsg.conn.connectionId);
        }

        private void Test(NetworkMessage netMsg) {
            var msg = new Msgs.Test();
            msg.Deserialize(netMsg.reader);
            print(msg.content);
        }

        private void Sync(NetworkMessage netMsg) {
            var msg = new Msgs.Sync();
            var test = msg.Deserialize<Snapshots.Test>(netMsg.reader);
            print(test.content);
        }
    }
}


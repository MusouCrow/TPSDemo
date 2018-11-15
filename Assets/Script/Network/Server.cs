using System;
using System.Collections.Generic;
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
        private Dictionary<int, List<Snapshot>> snapshotListMap;

        protected void Start() {
            bool ret = NetworkServer.Listen(this.port);

            if (ret) {
                NetworkServer.RegisterHandler(MsgType.Connect, this.OnClientConnected);
                NetworkServer.RegisterHandler(MsgType.Disconnect, this.OnClientDisconnected);
                NetworkServer.RegisterHandler(MsgTypes.Input, this.Input);

                this.snapshotListMap = new Dictionary<int, List<Snapshot>>();

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

        private void Input(NetworkMessage netMsg) {
            var id = netMsg.conn.connectionId;

            if (!this.snapshotListMap.ContainsKey(id)) {
                this.snapshotListMap.Add(id, new List<Snapshot>());
            }

            var msg = new Msgs.Input() {
                snapshotList = this.snapshotListMap[id]
            };
            msg.Deserialize(netMsg.reader);

            foreach (var s in this.snapshotListMap[id]) {
                print(s.frame + ", " + s.GetType().ToString());
            }

            this.snapshotListMap[id].Clear();
        }
    }
}


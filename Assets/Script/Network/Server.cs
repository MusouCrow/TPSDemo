using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using Random = UnityEngine.Random;

namespace Game.Network {
    using Actor;
    using Snapshots = Actor.Snapshots;

    public class Server : MonoBehaviour {
        private const int INTERVAL = 10;

        [SerializeField]
        private int port;
        private Dictionary<int, List<Snapshot>> snapshotListMap;
        private List<List<Snapshot>> syncList;
        private int frameCount;
        
        protected void Start() {
            bool ret = NetworkServer.Listen(this.port);
            
            if (ret) {
                NetworkServer.RegisterHandler(MsgType.Connect, this.OnClientConnected);
                NetworkServer.RegisterHandler(MsgType.Disconnect, this.OnClientDisconnected);
                NetworkServer.RegisterHandler(MsgTypes.Input, this.Input);

                this.snapshotListMap = new Dictionary<int, List<Snapshot>>();
                this.syncList = new List<List<Snapshot>>();

                print("Server Start");
            }
            else {
                Destroy(this);
            }
        }

        protected void FixedUpdate() {
            this.frameCount++;
            var list = new List<Snapshot>();
            
            foreach (var sl in this.snapshotListMap) {
                int frame = -1;

                while (sl.Value.Count > 0 && (frame == -1 || sl.Value[0].frame == frame)) {
                    var s = sl.Value[0];

                    s.connectionId = sl.Key;
                    frame = s.frame;
                    list.Add(s);
                    sl.Value.RemoveAt(0);
                }
            }

            if (list.Count > 0) {
                this.syncList.Add(list);
                Client.SyncList.Add(list);
            }

            if (this.frameCount % Server.INTERVAL == 0) {
                NetworkServer.SendToAll(MsgTypes.Sync, new Msgs.Sync() {
                    snapshotsList = this.syncList
                });

                this.syncList.Clear();
            }
        }

        private void OnClientConnected(NetworkMessage netMsg) {
            if (!Client.IsPlayer && NetworkServer.connections[1] == netMsg.conn) {
                return;
            }

            {
                var msg = new Msgs.Start() {
                    playerDatas = ActorMgr.ToPlayerDatas(),
                    connectionId = netMsg.conn.connectionId
                };
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
        }
    }
}


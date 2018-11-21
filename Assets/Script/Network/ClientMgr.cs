using System;
using System.Net;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Network {
    using Actor;

    public class ClientMgr : MonoBehaviour {
        private static ClientMgr INSTANCE;
        private const int INTERVAL = 5;

        public static void Input(Snapshot snapshot) {
            snapshot.fd = INSTANCE.fd;
            snapshot.frame = INSTANCE.frameCount;
            INSTANCE.sendList.Add(snapshot);
            INSTANCE.checkList.Add(snapshot);
        }

        public static string FD {
            get {
                return INSTANCE.fd;
            }
        }

        [SerializeField]
        private string address;
        [SerializeField]
        private int port;

        private Client client;
        private int frameCount;
        private bool start;
        private List<Snapshot> sendList;
        private List<Snapshot> checkList;
        private List<List<Snapshot>> syncList;
        private string fd;

        protected void Awake() {
            INSTANCE = this;

            this.sendList = new List<Snapshot>();
            this.checkList = new List<Snapshot>();
            this.syncList = new List<List<Snapshot>>();
            this.client = new Client();
        }

        protected void Start() {
            this.client.RegisterHandler(MsgId.Connect, this.Connected);
            this.client.RegisterHandler(MsgId.Disconnect, this.Disconnected);
            this.client.RegisterHandler(MsgId.Sync, this.Sync);
            this.client.RegisterHandler(MsgId.NewPlayer, this.NewPlayer);
            this.client.RegisterHandler(MsgId.DelPlayer, this.DelPlayer);

            this.client.Connect(this.address, this.port);
        }

        protected void FixedUpdate() {
            this.client.Update(Time.fixedDeltaTime);

            if (this.start && this.client.Active) {
                this.frameCount++;
                ClientMgr.Input(new Snapshot());

                if (this.frameCount % INTERVAL == 0) {
                    var msg = new Msg.Input() {
                        snapshotList = this.sendList
                    };
                    this.client.Send(MsgId.Input, msg);
                    this.sendList.Clear();
                }
            }
        }

        private void Connected(byte msgId, NetworkReader reader, IPEndPoint ep) {
            print("Client Connected");
            var msg = new Msg.Connect();
            msg.Deserialize(reader);
            this.fd = msg.fd;

            foreach (var p in msg.playerDatas) {
                ActorMgr.NewPlayer(p.fd, p.position, false);
            }

            this.start = true;
        }

        private void Disconnected(byte msgId, NetworkReader reader, IPEndPoint ep) {
            print("Client Disconnected");
        }

        private void NewPlayer(byte msgId, NetworkReader reader, IPEndPoint ep) {
            var playerData = new PlayerData();
            var msg = new Msg.NewPlayer() {playerData = playerData};
            msg.Deserialize(reader);
            ActorMgr.NewPlayer(playerData.fd, playerData.position, this.fd == playerData.fd);
        }

        private void DelPlayer(byte msgId, NetworkReader reader, IPEndPoint ep) {
            var msg = new Msg.DelPlayer();
            msg.Deserialize(reader);
            ActorMgr.DelPlayer(msg.fd);
        }

        private void Sync(byte msgId, NetworkReader reader, IPEndPoint ep) {
            var msg = new Msg.Sync() {
                syncList = this.syncList
            };
            msg.Deserialize(reader);

            var list = new List<Snapshot>();

            foreach (var sl in this.syncList) {
                foreach (var s in sl) {
                    if (s.fd == this.fd) {
                        list.Add(s);
                    }
                }
            }

            this.checkList.RemoveRange(0, list.Count);
            this.syncList.Clear();
        }
    }
}
using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Network {
    using Actor;

    public class ClientMgr : MonoBehaviour {
        private static ClientMgr INSTANCE;
        private const int INTERVAL = 5;

        public static void Input() {
            var snapshot = new Snapshot() {
                fd = INSTANCE.fd,
                frame = INSTANCE.frameCount
            };

            INSTANCE.sendList.Add(snapshot);
            INSTANCE.checkList.Add(snapshot);
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
        private StreamWriter streamWriter;

        public string fd {
            get;
            private set;
        }

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
            this.client.RegisterHandler(MsgId.Heartbeat, this.Heartbeat);
            this.client.RegisterHandler(MsgId.Sync, this.Sync);
            this.client.Connect(this.address, this.port);
        }

        protected void FixedUpdate() {
            this.client.Update(Time.fixedDeltaTime);

            if (this.start && this.client.Active) {
                this.frameCount++;
                ClientMgr.Input();

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
            this.start = true;

            this.streamWriter = new StreamWriter(this.fd + ".log", false);
        }

        private void Disconnected(byte msgId, NetworkReader reader, IPEndPoint ep) {
            print("Client Disconnected");
            this.streamWriter.Close();
        }

        private void Heartbeat(byte msgId, NetworkReader reader, IPEndPoint ep) {
            print("Client Heartbeat");
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

            for (int i = 0; i < list.Count; i++) {
                this.streamWriter.WriteLine(list[i].fd + ", " + list[i].frame + " | " + this.checkList[i].fd + ", " + this.checkList[i].frame);
            }

            this.checkList.RemoveRange(0, list.Count);
            this.syncList.Clear();
        }
    }
}
using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace Game.Network {
    using Actor;

    public class ClientMgr : MonoBehaviour {
        private const int INTERVAL = 5;
        private const int SYNCMAX = 15;
        private static ClientMgr INSTANCE;
        
        /*
        private static void Resolve(GameObject gameObject, List<Snapshot> list, int index) {
            for (int i = index; i < list.Count; i++) {
                list[i].Resolve(gameObject);
                
                if (i < list.Count - 1 && list[i].frame != list[i + 1].frame) {
                    gameObject.SendMessage("Simulate");
                }
            }

            gameObject.SendMessage("Simulate");
        } */

        public static void Input(string type, object obj) {
            var snapshot = new Snapshot() {
                fd = INSTANCE.fd,
                frame = INSTANCE.frameCount,
                type = type,
                obj = obj
            };
            INSTANCE.sendList.Add(snapshot);
            //INSTANCE.checkList.Add(snapshot);
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
        private List<Snapshot[]> syncList;
        private string fd;
        private StreamWriter writer;

        protected void Awake() {
            INSTANCE = this;

            this.sendList = new List<Snapshot>();
            this.checkList = new List<Snapshot>();
            this.syncList = new List<Snapshot[]>();
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
                //ClientMgr.Input("", null);

                if (this.syncList.Count > 0) {
                    this.Simulate();
                    
                    while(this.syncList.Count > SYNCMAX) {
                        this.client.Update(0);
                        this.Simulate();
                    }
                }

                if (this.frameCount % INTERVAL == 0) {
                    var msg = new Msg.Input() {
                        snapshots = this.sendList.ToArray()
                    };

                    this.client.Send(MsgId.Input, msg);
                    this.sendList.Clear();
                }
            }
        }

        protected void OnGUI() {
            GUILayout.Label(this.syncList.Count + ", " + this.frameCount);
        }

        private void Simulate() {
            foreach (var s in this.syncList[0]) {
                //if (s.fd != this.fd) {
                ActorMgr.Input(s);
                //}
                //this.writer.Write(s.Print() + " ");
            }
            
            //this.writer.Write("\n");
            ActorMgr.Simulate();
            this.syncList.RemoveAt(0);
        }

        private void Connected(byte msgId, string data, IPEndPoint ep) {
            var msg = JsonConvert.DeserializeObject<Msg.Connect>(data);
            this.fd = msg.fd;
            this.client.updateTime = msg.updateTime;

            foreach (var p in msg.playerDatas) {
                ActorMgr.NewPlayer(p.fd, p.position.ToV(), false);
            }

            this.writer = new StreamWriter(this.fd + ".log");
            this.start = true;
            print("Client Connected");
        }

        private void Disconnected(byte msgId, string data, IPEndPoint ep) {
            this.writer.Close();
            print("Client Disconnected");
        }

        private void NewPlayer(byte msgId, string data, IPEndPoint ep) {
            var playerData = JsonConvert.DeserializeObject<PlayerData>(data);
            ActorMgr.NewPlayer(playerData.fd, playerData.position.ToV(), this.fd == playerData.fd);
        }

        private void DelPlayer(byte msgId, string data, IPEndPoint ep) {
            var msg = JsonConvert.DeserializeObject<Msg.DelPlayer>(data);
            ActorMgr.DelPlayer(msg.fd);
        }

        private void Sync(byte msgId, string data, IPEndPoint ep) {
            var msg = JsonConvert.DeserializeObject<Msg.Sync>(data);
            this.writer.WriteLine(data);

            foreach (var ss in msg.snapshotses) {
                this.syncList.Add(ss);
            }

            /*
            var scale = this.syncList.Count - RANGE;
            Time.timeScale = scale < 1 ? 1 : scale;

            if (scale > 1) {
                print(scale);
            } */

            /*
            var list = new List<Snapshot>();

            foreach (var sl in this.syncList) {
                bool hasAdded = false;

                foreach (var s in sl) {
                    if (this.fd == s.fd) {
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

            if (index == list.Count) { // Agreement
                this.checkList.RemoveRange(0, list.Count);
            }
            else {
                var player = ActorMgr.GetPlayer(this.fd);
                var frame = list[list.Count - 1].frame;

                for (int i = this.checkList.Count - 1; i >= 0; i--) {
                    if (this.checkList[i].frame <= frame) {
                        this.checkList.RemoveAt(i);
                    }
                }

                ClientMgr.Resolve(player, list, index);
                ClientMgr.Resolve(player, this.checkList, 0);
            } */
        }
    }
}
using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Network {
    using Actor;

    public class ClientMgr : MonoBehaviour {
        private const int INTERVAL = 5;
        private const int SYNCMAX = 15;
        private static ClientMgr INSTANCE;
        
        private static void Resolve(string fd, List<Snapshot> list, int index) {
            for (int i = index; i < list.Count; i++) {
                ActorMgr.Input(list[i]);

                if (i < list.Count - 1 && list[i].frame != list[i + 1].frame) {
                    ActorMgr.Simulate(fd);
                }
            }

            ActorMgr.Simulate(fd);
        }

        public static void Input(Snapshot snapshot) {
            snapshot.fd = INSTANCE.fd;
            snapshot.frame = INSTANCE.frameCount;
            INSTANCE.sendList.Add(snapshot);
            INSTANCE.checkList.Add(snapshot);

            if (INSTANCE.laterSendFrame != INSTANCE.frameCount) {
                INSTANCE.snapshotFrameCount++;
            }
        }

        public static void AddSync(List<Snapshot> list) {
            INSTANCE.syncList.Add(list);
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
        private int laterSendFrame;
        private int snapshotFrameCount;
        private StreamWriter writer;

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
                ActorMgr.Simulate(this.fd);

                if (this.syncList.Count > 0) {
                    this.Simulate();
                    
                    while(this.syncList.Count > SYNCMAX) {
                        this.client.Update(0);
                        this.Simulate();
                    }
                }

                if (this.frameCount % INTERVAL == 0) {
                    var msg = new Msg.Input() {
                        snapshotFrameCount = this.snapshotFrameCount,
                        snapshotList = this.sendList
                    };
                    this.client.Send(MsgId.Input, msg);
                    this.sendList.Clear();
                    this.snapshotFrameCount = 0;
                }
            }
        }

        protected void OnGUI() {
            ActorMgr.Position();
        }

        private void Simulate() {
            foreach (var s in this.syncList[0]) {
                ActorMgr.Input(s);
            }

            string laterFd = null;

            foreach (var s in this.syncList[0]) {
                if (laterFd != s.fd) {
                    ActorMgr.Simulate(s.fd);
                    laterFd = s.fd;
                }
            }
            
            this.syncList.RemoveAt(0);
        }

        private void Connected(byte msgId, NetworkReader reader, IPEndPoint ep) {
            print("Client Connected");
            var msg = new Msg.Connect();
            msg.Deserialize(reader);
            this.fd = msg.fd;

            foreach (var p in msg.playerDatas) {
                ActorMgr.NewPlayer(p.fd, p.position, false);
            }

            //this.writer = new StreamWriter(this.fd + ".log");
            this.start = true;
        }

        private void Disconnected(byte msgId, NetworkReader reader, IPEndPoint ep) {
            print("Client Disconnected");
            //this.writer.Close();
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
            if (ServerMgr.Active) {
                return;
            }

            var msg = new Msg.Sync() {syncList = this.syncList};
            msg.Deserialize(reader, this.fd);
            List<Snapshot> list = msg.selfList;
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
                var frame = list[list.Count - 1].frame;

                for (int i = this.checkList.Count - 1; i >= 0; i--) {
                    if (this.checkList[i].frame <= frame) {
                        this.checkList.RemoveAt(i);
                    }
                }

                ClientMgr.Resolve(this.fd, list, index);
                ClientMgr.Resolve(this.fd, this.checkList, 0);
            }
        }
    }
}
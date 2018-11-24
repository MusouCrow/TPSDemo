using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

using Random = UnityEngine.Random;

namespace Game.Network {
    using Actor;
    using Utility;

    public class ServerMgr : MonoBehaviour {
        private const int INTERVAL = 10;

        [SerializeField]
        private int port;
        private Server server;
        private int frameCount;
        private Dictionary<string, List<Snapshot>> snapshotListMap;
        private List<Snapshot[]> syncList;
        //private StreamWriter writer;

        protected void Awake() {
            this.server = new Server();
            this.snapshotListMap = new Dictionary<string, List<Snapshot>>();
            this.syncList = new List<Snapshot[]>();
        }

        protected void Start() {
            this.server.RegisterHandler(MsgId.Connect, this.NewConnection);
            this.server.RegisterHandler(MsgId.Disconnect, this.DelConnection);
            this.server.RegisterHandler(MsgId.Input, this.Input);
            this.server.Listen(this.port);

            if (!this.server.Active) {
                Destroy(this);
                return;
            }

            //this.writer = new StreamWriter("server.log");
        }

        protected void FixedUpdate() {
            this.server.Update(Time.fixedDeltaTime);
            
            if (this.server.Active) {
                this.frameCount++;

                var list = new List<Snapshot>();
                
                foreach (var sl in this.snapshotListMap) {
                    int frame = -1;

                    while (sl.Value.Count > 0 && (frame == -1 || sl.Value[0].frame == frame)) {
                        var s = sl.Value[0];
                        frame = s.frame;
                        list.Add(s);
                        sl.Value.RemoveAt(0);
                    }
                }
                
                if (list.Count > 0) {
                    /*
                    foreach (var s in list) {
                        this.writer.Write(s.Print() + " ");
                    }

                    this.writer.Write("\n"); */
                    this.syncList.Add(list.ToArray());
                }

                if (this.frameCount % INTERVAL == 0) {
                    var msg = new Msg.Sync() {
                        snapshotses = this.syncList.ToArray()
                    };

                    this.server.SendToAll(MsgId.Sync, msg);
                    this.syncList.Clear();
                }
                
                if (UnityEngine.Input.GetKeyDown(KeyCode.Space)) {
                    this.server.Close();
                    //this.writer.Close();
                }
            }
        }

        private void NewConnection(byte msgId, string data, IPEndPoint ep) {
            var fd = ep.ToString();
            
            {
                var msg = new Msg.Connect() {
                    fd = fd,
                    updateTime = this.server.updateTime,
                    playerDatas = ActorMgr.ToPlayerDatas()
                };
                
                this.server.Send(ep, MsgId.Connect, msg);
            }
            
            {
                var x = Mathf.Lerp(-2, 2, Random.value);
                var z = Mathf.Lerp(-2, 2, Random.value);
                var playerData = new PlayerData() {
                    fd = fd,
                    position = new NVector3(x, 0, z)
                };

                this.server.SendToAll(MsgId.NewPlayer, playerData);
            }

            print("New Client: " + fd);
        }

        private void DelConnection(byte msgId, string data, IPEndPoint ep) {
            var fd = ep.ToString();
            var msg = new Msg.DelPlayer() {
                fd = fd
            };
            this.server.SendToAll(MsgId.DelPlayer, msg);
            
            print("Del Client: " + fd);
        }

        private void Input(byte msgId, string data, IPEndPoint ep) {
            var fd = ep.ToString();
            var msg = JsonConvert.DeserializeObject<Msg.Input>(data);

            if (!this.snapshotListMap.ContainsKey(fd)) {
                this.snapshotListMap.Add(fd, new List<Snapshot>());
            }
            
            foreach (var s in msg.snapshots) {
                this.snapshotListMap[fd].Add(s);
            }
        }
    }
}
using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Network {
    using Actor;

    public class ServerMgr : MonoBehaviour {
        private const int INTERVAL = 10;

        [SerializeField]
        private int port;

        private Server server;
        private int frameCount;
        private Dictionary<string, List<Snapshot>> snapshotListMap;
        private List<List<Snapshot>> syncList;
        private StreamWriter streamWriter;

        protected void Awake() {
            this.server = new Server();
            this.snapshotListMap = new Dictionary<string, List<Snapshot>>();
            this.syncList = new List<List<Snapshot>>();
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

            this.streamWriter = new StreamWriter("server.log", false);
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
                    this.syncList.Add(list);
                }

                if (this.frameCount % INTERVAL == 0) {
                    this.server.SendToAll(MsgId.Sync, new Msg.Sync() {syncList = this.syncList});
                    /*
                    foreach (var sl in this.syncList) {
                        this.streamWriter.Write(this.frameCount + ": ");

                        foreach (var s in sl) {
                            this.streamWriter.Write("(" + s.fd + ", " + s.frame + ") ");
                        }

                        this.streamWriter.Write("\n");
                    } */

                    this.syncList.Clear();
                }

                if (UnityEngine.Input.GetKeyDown(KeyCode.Space)) {
                    this.server.Close();
                    //this.streamWriter.Close();
                }
            }
        }

        private void NewConnection(byte msgId, NetworkReader reader, IPEndPoint ep) {
            var fd = ep.ToString();
            print("New Client: " + fd);
            this.server.Send(ep, MsgId.Connect, new Msg.Connect() {fd = fd});
        }

        private void DelConnection(byte msgId, NetworkReader reader, IPEndPoint ep) {
            print("Del Client: " + ep.ToString());
        }

        private void Input(byte msgId, NetworkReader reader, IPEndPoint ep) {
            var fd = ep.ToString();

            if (!this.snapshotListMap.ContainsKey(fd)) {
                this.snapshotListMap.Add(fd, new List<Snapshot>());
            }
            
            var msg = new Msg.Input() {
                snapshotList = this.snapshotListMap[fd]
            };
            msg.Deserialize(reader);
        }
    }
}
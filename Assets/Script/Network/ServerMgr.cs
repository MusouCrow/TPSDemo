using System;
using System.Net;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using Random = UnityEngine.Random;

namespace Game.Network {
    using Actor;

    public class ServerMgr : MonoBehaviour {
        private const int INTERVAL = 10;
        private static ServerMgr INSTANCE;

        private class Unit {
            public List<Snapshot> list;
            public int count;

            public Unit() {
                this.list = new List<Snapshot>();
            }
        }

        public static bool Active {
            get {
                return INSTANCE != null && INSTANCE.server.Active;
            }
        }

        public static bool IsPlayer {
            get {
                return INSTANCE != null && INSTANCE.isPlayer;
            }
        }

        public static void Input(string fd, Snapshot snapshot, bool fromServer) {
            snapshot.fd = fd;
            snapshot.frame = INSTANCE.frameCount;
            snapshot.fromServer = fromServer;
            var unitMap = INSTANCE.unitMap;

            if (!unitMap.ContainsKey(fd)) {
                unitMap.Add(fd, new Unit());
            }

            unitMap[fd].count++;
            unitMap[fd].list.Insert(0, snapshot);
        }

        [SerializeField]
        private int port;
        [SerializeField]
        private bool isPlayer;
        private Server server;
        private int frameCount;
        private Dictionary<string, Unit> unitMap;
        private List<List<Snapshot>> syncList;

        protected void Awake() {
            INSTANCE = this;

            this.server = new Server();
            this.unitMap = new Dictionary<string, Unit>();
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
        }

        protected void FixedUpdate() {
            this.server.Update(Time.fixedDeltaTime);
            
            if (this.server.Active) {
                this.frameCount++;

                if (this.frameCount % INTERVAL == 0) {
                    this.server.SendToAll(MsgId.Sync, new Msg.Sync() {syncList = this.syncList});
                    this.syncList.Clear();
                }

                var list = new List<Snapshot>();
                
                foreach (var i in this.unitMap) {
                    int frame = -1;
                    var sl = i.Value.list;

                    while (sl.Count > 0 && (i.Value.count > INTERVAL || (frame == -1 || sl[0].frame == frame))) {
                        var s = sl[0];
                        list.Add(s);
                        sl.RemoveAt(0);

                        if (frame != s.frame) {
                            frame = s.frame;
                            i.Value.count--;
                        }
                    }
                }
                
                if (list.Count > 0) {
                    ClientMgr.AddSync(list);
                    this.syncList.Add(list);
                }
            }
        }

        private void NewConnection(byte msgId, NetworkReader reader, IPEndPoint ep) {
            var fd = ep.ToString();
            
            {
                var msg = new Msg.Connect() {
                    fd = fd,
                    playerDatas = ActorMgr.ToPlayerDatas()
                };
                
                this.server.Send(ep, MsgId.Connect, msg);
            }
            
            if (!this.isPlayer && this.server.ConnectionCount == 1) {
                return;
            }

            {
                var x = Mathf.Lerp(-2, 2, Random.value);
                var z = Mathf.Lerp(-2, 2, Random.value);

                var msg = new Msg.NewPlayer() {
                    playerData = new PlayerData() {
                        fd = fd,
                        position = new Vector3(x, 1, z)
                    }
                };

                this.server.SendToAll(MsgId.NewPlayer, msg);
            }

            print("New Client: " + fd);
        }

        private void DelConnection(byte msgId, NetworkReader reader, IPEndPoint ep) {
            var fd = ep.ToString();
            var msg = new Msg.DelPlayer() {
                fd = fd
            };
            this.server.SendToAll(MsgId.DelPlayer, msg);
            
            print("Del Client: " + fd);
        }

        private void Input(byte msgId, NetworkReader reader, IPEndPoint ep) {
            var fd = ep.ToString();

            if (!this.unitMap.ContainsKey(fd)) {
                this.unitMap.Add(fd, new Unit());
            }
            
            var msg = new Msg.Input() {
                snapshotList = this.unitMap[fd].list
            };
            msg.Deserialize(reader);
            this.unitMap[fd].count += msg.snapshotFrameCount;
        }
    }
}
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Network {
    using Actor;

    public static class MsgId {
        public const byte Disconnect = 0;
        public const byte Connect = 1;
        public const byte Heartbeat = 2;
        public const byte NewPlayer = 3;
        public const byte DelPlayer = 4;
        public const byte Input = 5;
        public const byte Sync = 6;
    }

    namespace Msg {
        public class Connect : MessageBase {
            public string fd;
            public PlayerData[] playerDatas;

            public override void Serialize(NetworkWriter writer) {
                writer.Write(this.fd);
                writer.Write(this.playerDatas.Length);

                foreach (var p in this.playerDatas) {
                    p.Serialize(writer);
                }
            }

            public override void Deserialize(NetworkReader reader) {
                this.fd = reader.ReadString();
                int length = reader.ReadInt32();
                this.playerDatas = new PlayerData[length];

                for (int i=0; i<length; i++) {
                    this.playerDatas[i] = new PlayerData();
                    this.playerDatas[i].Deserialize(reader);
                }
            }
        }

        public class NewPlayer : MessageBase {
            public PlayerData playerData;
            
            public override void Serialize(NetworkWriter writer) {
                playerData.Serialize(writer);
            }

            public override void Deserialize(NetworkReader reader) {
                playerData.Deserialize(reader);
            }
        }

        public class DelPlayer : MessageBase {
            public string fd;

            public override void Serialize(NetworkWriter writer) {
                writer.Write(this.fd);
            }

            public override void Deserialize(NetworkReader reader) {
                this.fd = reader.ReadString();
            }   
        }

        public class Input : MessageBase {
            public int snapshotFrameCount;
            public List<Snapshot> snapshotList;

            public override void Serialize(NetworkWriter writer) {
                writer.Write(this.snapshotFrameCount);
                writer.Write(this.snapshotList.Count);

                foreach (var s in this.snapshotList) {
                    s.Serialize(writer, false);
                }
            }

            public override void Deserialize(NetworkReader reader) {
                this.snapshotFrameCount = reader.ReadInt32();
                int count = reader.ReadInt32();
                var assembly = Assembly.GetExecutingAssembly();

                for (int i = 0; i < count; i++) {
                    var type = reader.ReadString();
                    var s = assembly.CreateInstance(type) as Snapshot;
                    s.Deserialize(reader, false);
                    this.snapshotList.Add(s);
                }
            }
        }

        public class Sync : MessageBase {
            public List<List<Snapshot>> syncList;
            public List<Snapshot> selfList;
            public List<Snapshot> serverList;

            public override void Serialize(NetworkWriter writer) {
                writer.Write(this.syncList.Count);

                foreach (var sl in this.syncList) {
                    writer.Write(sl.Count);

                    foreach (var s in sl) {
                        s.Serialize(writer, true);
                    }
                }
            }

            public void Deserialize(NetworkReader reader, string fd) {
                base.Deserialize(reader);

                int count = reader.ReadInt32();
                var assembly = Assembly.GetExecutingAssembly();
                this.selfList = new List<Snapshot>();
                this.serverList = new List<Snapshot>();

                for (int i = 0; i < count; i++) {
                    var list = new List<Snapshot>();
                    int len = reader.ReadInt32();

                    for (int j = 0; j < len; j++) {
                        var type = reader.ReadString();
                        var s = assembly.CreateInstance(type) as Snapshot;
                        s.Deserialize(reader, true);
                        
                        List<Snapshot> sl;

                        if (s.fd == fd) {
                            sl = s.fromServer ? this.serverList : this.selfList;
                        }
                        else {
                            sl = list;
                        }

                        sl.Add(s);
                    }

                    this.syncList.Add(list);
                }
            }
        }
    }
}
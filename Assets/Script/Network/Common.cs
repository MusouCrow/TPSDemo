using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Network {
    using Actor;

    public class MsgTypes {
        public const short Test = 1001;
        public const short NewPlayer = 1002;
        public const short DelPlayer = 1003;
        public const short Start = 1004;
        public const short Input = 1005;
        public const short Sync = 1006;
    }

    namespace Msgs {
        public class Test : MessageBase {
            public string content;

            public override void Serialize(NetworkWriter writer) {
                writer.Write(this.content);
            }

            public override void Deserialize(NetworkReader reader) {
                this.content = reader.ReadString();
            }
        }

        public class NewPlayer : MessageBase {
            public int connectionId;
            public Vector3 position;

            public override void Serialize(NetworkWriter writer) {
                writer.Write(this.connectionId);
                writer.Write(this.position);
            }

            public override void Deserialize(NetworkReader reader) {
                this.connectionId = reader.ReadInt32();
                this.position = reader.ReadVector3();
            }
        }

        public class DelPlayer : MessageBase {
            public int connectionId;

            public override void Serialize(NetworkWriter writer) {
                writer.Write(this.connectionId);
            }

            public override void Deserialize(NetworkReader reader) {
                this.connectionId = reader.ReadInt32();
            }
        }

        public class Start : MessageBase {
            public PlayerData[] playerDatas;

            public override void Serialize(NetworkWriter writer) {
                writer.Write(this.playerDatas.Length);

                foreach (var p in this.playerDatas) {
                    writer.Write(p.connectionId);
                    writer.Write(p.position);
                }
            }

            public override void Deserialize(NetworkReader reader) {
                int length = reader.ReadInt32();
                this.playerDatas = new PlayerData[length];

                for (int i=0; i<length; i++) {
                    this.playerDatas[i] = new PlayerData() {
                        connectionId = reader.ReadInt32(),
                        position = reader.ReadVector3()
                    };
                }
            }
        }

        public class Input : MessageBase {
            public List<Snapshot> snapshotList;

            public override void Serialize(NetworkWriter writer) {
                writer.Write(this.snapshotList.Count);

                foreach (var s in this.snapshotList) {
                    writer.Write(s.GetType().ToString());
                    writer.Write(s.frame);
                    s.Serialize(writer);
                }
            }
            
            public override void Deserialize(NetworkReader reader) {
                int length = reader.ReadInt32();
                var assembly = Assembly.GetExecutingAssembly();
                
                for (int i=0; i < length; i++) {
                    var type = reader.ReadString();
                    var snapshot = assembly.CreateInstance(type) as Snapshot;
                    snapshot.frame = reader.ReadInt32();
                    snapshot.Deserialize(reader);
                    this.snapshotList.Add(snapshot);
                }
            }
        }
        /*
        public class Sync : MessageBase {
            public Dictionary<int, List<Snapshot>> snapshotListMap;

            public override void Serialize(NetworkWriter writer) {
                writer.Write(this.snapshotListMap.Count);

                foreach (var sl in this.snapshotListMap) {
                    writer.Write(sl.Key);
                    
                    foreach (var s in sl.Value) {
                        writer.Write(s.frame);
                        s.Serialize(writer);
                    }
                }
            }

            public void Deserialize<T>(NetworkReader reader) where T : Snapshot, new() {

            }
        } */
    }
}
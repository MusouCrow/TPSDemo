using System;
using System.Collections.Generic;
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
            public List<Snapshot> snapshotList;

            public override void Serialize(NetworkWriter writer) {
                writer.Write(this.snapshotList.Count);

                foreach (var s in this.snapshotList) {
                    s.Serialize(writer, false);
                }
            }

            public override void Deserialize(NetworkReader reader) {
                int count = reader.ReadInt32();

                for (int i = 0; i < count; i++) {
                    var s = new Snapshot();
                    s.Deserialize(reader, false);
                    this.snapshotList.Add(s);
                }
            }
        }

        public class Sync : MessageBase {
            public List<List<Snapshot>> syncList;

            public override void Serialize(NetworkWriter writer) {
                writer.Write(this.syncList.Count);

                foreach (var sl in this.syncList) {
                    writer.Write(sl.Count);

                    foreach (var s in sl) {
                        s.Serialize(writer, true);
                    }
                }
            }

            public override void Deserialize(NetworkReader reader) {
                int count = reader.ReadInt32();

                for (int i = 0; i < count; i++) {
                    var list = new List<Snapshot>();
                    int len = reader.ReadInt32();

                    for (int j = 0; j < len; j++) {
                        var s = new Snapshot();
                        s.Deserialize(reader, true);
                        list.Add(s);
                    }

                    this.syncList.Add(list);
                }
            }
        }
    }
}
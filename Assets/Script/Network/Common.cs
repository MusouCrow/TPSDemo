using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace Game.Network {
    using Actor;

    public static class MsgId {
        public const byte Disconnect = 0;
        public const byte Connect = 1;
        public const byte Heartbeat = 2;
        public const byte Input = 3;
        public const byte Sync = 4;
    }

    namespace Msg {
        public class Connect : MessageBase {
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
                    s.Serialize(writer);
                }
            }

            public override void Deserialize(NetworkReader reader) {
                int count = reader.ReadInt32();

                for (int i = 0; i < count; i++) {
                    var s = new Snapshot();
                    s.Deserialize(reader);
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
                        s.Serialize(writer);
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
                        s.Deserialize(reader);
                        list.Add(s);
                    }

                    this.syncList.Add(list);
                }
            }
        }
    }
}
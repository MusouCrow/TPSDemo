using UnityEngine;
using UnityEngine.Networking;

namespace Game.Actor {
    public class PlayerData {
        public int connectionId;
        public Vector3 position;
    }

    public class Snapshot {
        public int frame;
        public int connectionId;

        public virtual void Serialize(NetworkWriter writer, bool isFull) {}
        public virtual void Deserialize(NetworkReader reader, bool isFull) {}
    }

    namespace Snapshots {
        public class Test : Snapshot {
            public string content;

            public override void Serialize(NetworkWriter writer, bool isFull) {
                writer.Write(this.content);
            }

            public override void Deserialize(NetworkReader reader, bool isFull) {
                this.content = reader.ReadString();
            }
        }

        public class ChangeColor : Snapshot {
            public bool isWhite;

            public override void Serialize(NetworkWriter writer, bool isFull) {
                if (isFull) {
                    writer.Write(this.isWhite);
                }
            }

            public override void Deserialize(NetworkReader reader, bool isFull) {
                if (isFull) {
                    this.isWhite = reader.ReadBoolean();
                }
            }
        }
    }
}
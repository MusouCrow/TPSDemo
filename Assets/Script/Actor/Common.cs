using UnityEngine;
using UnityEngine.Networking;

namespace Game.Actor {
    public class PlayerData {
        public int connectionId;
        public Vector3 position;
    }

    public class Snapshot {
        public int frame;

        public virtual void Serialize(NetworkWriter writer) {}
        public virtual void Deserialize(NetworkReader reader) {}
    }

    namespace Snapshots {
        public class Test : Snapshot {
            public string content;

            public override void Serialize(NetworkWriter writer) {
                writer.Write(this.content);
            }

            public override void Deserialize(NetworkReader reader) {
                this.content = reader.ReadString();
            }
        }
    }
}
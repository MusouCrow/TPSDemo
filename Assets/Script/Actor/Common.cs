using UnityEngine;
using UnityEngine.Networking;

namespace Game.Actor {
    public class PlayerData {
        public string fd;
        public Vector3 position;

        public void Serialize(NetworkWriter writer) {
            writer.Write(this.fd);
            writer.Write(this.position);
        }

        public void Deserialize(NetworkReader reader) {
            this.fd = reader.ReadString();
            this.position = reader.ReadVector3();
        }
    }

    public class Snapshot {
        public string fd;
        public int frame;

        public virtual void Serialize(NetworkWriter writer) {
            writer.Write(this.fd);
            writer.Write(this.frame);
        }

        public virtual void Deserialize(NetworkReader reader) {
            this.fd = reader.ReadString();
            this.frame = reader.ReadInt32();
        }

        public virtual bool Equals(Snapshot snapshot) {
            return this.fd == snapshot.fd && this.frame == snapshot.frame;
        }
    }
}
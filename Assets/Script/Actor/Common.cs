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

        public virtual void Serialize(NetworkWriter writer, bool isFull) {
            writer.Write(this.GetType().ToString());
            writer.Write(this.fd);
            writer.Write(this.frame);
        }

        public virtual void Deserialize(NetworkReader reader, bool isFull, int index) {
            this.fd = reader.ReadString();
            //Debug.Log(index + "_" + this.fd);
            this.frame = reader.ReadInt32();
            //Debug.Log(index + "_" + this.frame);
        }

        public virtual bool Equals(Snapshot snapshot) {
            return this.fd == snapshot.fd && this.frame == snapshot.frame;
        }

        public virtual string Print() {
            return "(" + this.fd + ", " + this.frame + ", " + this.ToString() + ")";
        }
    }

    namespace Snapshots {
        public class Move : Snapshot {
            public Vector3 velocity;
            public Vector3 position;
            
            public override void Serialize(NetworkWriter writer, bool isFull) {
                base.Serialize(writer, isFull);

                writer.Write(this.velocity);

                if (isFull) {
                    writer.Write(this.position);
                }
            }

            public override void Deserialize(NetworkReader reader, bool isFull, int index) {
                base.Deserialize(reader, isFull, index);
                
                this.velocity = reader.ReadVector3();
                
                if (isFull) {
                    this.position = reader.ReadVector3();
                }

                //Debug.Log(index + "_" + this.velocity + ", " + this.position);
            }
        }
    }
}
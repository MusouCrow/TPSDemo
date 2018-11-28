using System;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Network {
    using Actor;

    public class Test : MonoBehaviour {
        public void Start() {
            var move = new Actor.Snapshots.Move() {
                velocity = Vector3.zero,
                position = this.transform.position
            };

            var writer = new NetworkWriter();
            move.Serialize(writer, true);

            byte[] com;
            byte[] decom;
            var data = writer.ToArray();
            print(data.Length);

            using (var ms = new MemoryStream()) {
                using(var ds = new DeflateStream(ms, CompressionMode.Compress)) {
                    ds.Write(data, 0, data.Length);
                }

                com = ms.ToArray();
            }

            print(com.Length);

            using (var ms = new MemoryStream(com)) {
                using(var ds = new DeflateStream(ms, CompressionMode.Decompress)) {
                    var ws = new MemoryStream();
                    int count;
                    var buffer = new byte[4096];

                    while((count = ds.Read(buffer, 0, buffer.Length)) != 0) {
                        ws.Write(buffer, 0, count);
                    }

                    decom = ws.ToArray();
                }
            }

            print(decom.Length);
        }
    }
}
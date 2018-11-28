using System;
using System.Net;
using UnityEngine;

namespace Game.Network {
    using Utility;

    public class Connection {
        private KCP kcp;
        private Action<byte, byte[]> Handle;

        public bool heartbeat;
        public IPEndPoint EndPoint {
            private set;
            get;
        }

        public Connection(IPEndPoint endPoint, Action<IPEndPoint, byte[], int> Send, Action<IPEndPoint, byte, byte[]> Handle) {
            this.EndPoint = endPoint;
            this.heartbeat = true;
            
            this.kcp = new KCP(1, (byte[] buffer, int size) => Send(this.EndPoint, buffer, size));
            this.kcp.NoDelay(1, 10, 2, 1);
            this.kcp.WndSize(128, 128);

            this.Handle = (byte id, byte[] data) => Handle(this.EndPoint, id, data);
        }

        public void Update(uint current) {
            this.kcp.Update(current);
            
            for (var size = this.kcp.PeekSize(); size > 0; size = this.kcp.PeekSize()) {
                var buffer = new byte[size];

                if (this.kcp.Recv(buffer) > 0) {
                    var id = buffer[0];
                    var data = new byte[buffer.Length - 1];

                    for (int i = 0; i < data.Length; i++) {
                        data[i] = buffer[i + 1];
                    }
                    
                    this.Handle(id, Math.Decompress(data));
                }
            }
        }

        public void Send(byte[] buffer) {
            this.kcp.Send(buffer);
        }

        public void Input(byte[] buffer) {
            this.heartbeat = true;
            this.kcp.Input(buffer);
        }
    }
}
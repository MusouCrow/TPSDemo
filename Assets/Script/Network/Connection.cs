using System;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace Game.Network {
    public class Connection {
        private KCP kcp;
        private Action<byte, string> Handle;

        public bool heartbeat;
        public IPEndPoint EndPoint {
            private set;
            get;
        }

        public Connection(IPEndPoint endPoint, Action<IPEndPoint, byte[], int> Send, Action<IPEndPoint, byte, string> Handle) {
            this.EndPoint = endPoint;
            this.heartbeat = true;
            
            this.kcp = new KCP(1, (byte[] buffer, int size) => Send(this.EndPoint, buffer, size));
            this.kcp.NoDelay(1, 10, 2, 1);
            this.kcp.WndSize(128, 128);

            this.Handle = (byte id, string data) => Handle(this.EndPoint, id, data);
        }

        public void Update(uint current) {
            this.kcp.Update(current);
            
            for (var size = this.kcp.PeekSize(); size > 0; size = this.kcp.PeekSize()) {
                var buffer = new byte[size];

                if (this.kcp.Recv(buffer) > 0) {
                    byte id = buffer[0];
                    string data = Encoding.UTF8.GetString(buffer, 1, buffer.Length - 1);
                    this.Handle(id, data);
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
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Network {
    public class Client {
        private static IPEndPoint EP;

        private UdpClient udp;
        private KCP kcp;
        private float updateTime;
        private Dictionary<byte, Action<byte, NetworkReader>> handlerMap;

        public bool Connected {
            get;
            private set;
        }

        public Client() {
            this.handlerMap = new Dictionary<byte, Action<byte, NetworkReader>>();
        }

        public void Update(float dt) {
            if (!this.Connected) {
                return;
            }

            this.updateTime += dt;
            this.kcp.Update((uint)Mathf.FloorToInt(this.updateTime * 1000));

            for (var size = this.kcp.PeekSize(); size > 0; size = this.kcp.PeekSize()) {
                var buffer = new byte[size];

                if (this.kcp.Recv(buffer) > 0) {
                    byte id = buffer[0];
                    byte[] data = new byte[buffer.Length - 1];

                    for (int i = 0; i < data.Length; i++) {
                        data[i] = buffer[i + 1];
                    }

                    this.Handle(id, data);
                }
            }
        }

        public bool Connect(string address, int port) {
            if (this.Connected) {
                return false;
            }

            this.udp = new UdpClient(address, port);
            this.kcp = new KCP(1, this.SendWrap);
            this.kcp.NoDelay(1, 10, 2, 1);
            this.kcp.WndSize(128, 128);
            this.Send(MsgId.Connect);
            this.Receive();
            this.updateTime = 0;
            this.Connected = true;

            return true;
        }

        public bool Disconnect() {
            if (!this.Connected) {
                return false;
            }

            this.Connected = false;
            this.udp.Close();
            this.Handle(MsgId.Disconnect, null);

            return true;
        }

        public void Send(byte id, MessageBase message=null) {
            byte[] buffer;

            if (message != null) {
                var writer = new NetworkWriter();
                message.Serialize(writer);

                var data = writer.AsArray();
                buffer = new byte[data.Length + 1];
                buffer[0] = id;

                for (int i = 0; i < data.Length; i++) {
                    buffer[i + 1] = data[i];
                }
            }
            else {
                buffer = new byte[] {id};
            }

            this.kcp.Send(buffer);
        }

        public void RegisterHandler(byte id, Action<byte, NetworkReader> Func) {
            this.handlerMap.Add(id, Func);
        }

        private void Handle(byte id, byte[] data) {
            var reader = new NetworkReader(data);

            if (this.handlerMap.ContainsKey(id)) {
                this.handlerMap[id](id, reader);
            }
        }

        private void Receive() {
            this.udp.BeginReceive(this.ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult ar) {
            try {
                var data = this.udp.EndReceive(ar, ref EP);

                if (data != null) {
                    this.kcp.Input(data);
                }

                this.Receive();
            }
            catch (SocketException) {
                this.Disconnect();
            }
        }

        private void SendCallback(IAsyncResult ar) {
            this.udp.EndSend(ar);
        }

        private void SendWrap(byte[] data, int size) {
            try {
                this.udp.BeginSend(data, size, this.SendCallback, null);
            }
            catch (SocketException) {
                this.Disconnect();
            }
        }
    }
}

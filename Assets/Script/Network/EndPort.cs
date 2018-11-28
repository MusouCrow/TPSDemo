using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Network {
    using Utility;

    public abstract class EndPort {
        private const float HEARTBEAT_INTERVAL = 3;

        protected UdpClient udp;
        protected float updateTime;
        protected Timer heartbeatTimer;
        private Dictionary<byte, Action<byte, NetworkReader, IPEndPoint>> handlerMap;

        public bool Active {
            get;
            protected set;
        }

        public EndPort() {
            this.handlerMap = new Dictionary<byte, Action<byte, NetworkReader, IPEndPoint>>();
            this.heartbeatTimer = new Timer(HEARTBEAT_INTERVAL, this.HeartbeatTick);
        }

        protected bool Init() {
            if (this.Active) {
                return false;
            }

            this.updateTime = 0;
            this.Active = true;
            this.heartbeatTimer.Enter();

            return true;
        }

        public virtual bool Close() {
            if (!this.Active) {
                return false;
            }

            this.Active = false;
            this.udp.Close();

            return true;
        }

        public abstract void Update(float dt);

        public void RegisterHandler(byte id, Action<byte, NetworkReader, IPEndPoint> Func) {
            this.handlerMap.Add(id, Func);
        }

        protected uint ToKCPClock() {
            return (uint)Mathf.FloorToInt(this.updateTime * 1000);
        } 

        protected void Handle(IPEndPoint ep, byte id, byte[] data) {
            var reader = new NetworkReader(data);

            if (this.handlerMap.ContainsKey(id)) {
                this.handlerMap[id](id, reader, ep);
            }
        }
        
        protected void Send(Connection connection, byte id, MessageBase message=null) {
            byte[] buffer;

            if (message != null) {
                var writer = new NetworkWriter();
                message.Serialize(writer);

                var data = Math.Compress(writer.AsArray());
                buffer = new byte[data.Length + 1];
                buffer[0] = id;

                for (int i = 0; i < data.Length; i++) {
                    buffer[i + 1] = data[i];
                }
            }
            else {
                buffer = new byte[] {id};
            }

            connection.Send(buffer);
        }

        protected void Receive() {
            this.udp.BeginReceive(this.ReceiveCallback, null);
        }

        protected virtual void SendWrap(IPEndPoint ep, byte[] buffer, int size) {
            this.udp.BeginSend(buffer, size, ep, this.SendCallback, null);
        }

        protected abstract void ReceiveCallback(IAsyncResult ar);

        protected void SendCallback(IAsyncResult ar) {
            this.udp.EndSend(ar);
        }

        protected abstract void HeartbeatTick();
    }
}
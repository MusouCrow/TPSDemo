using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace Game.Network {
    using Utility;

    public abstract class EndPort {
        private const float HEARTBEAT_INTERVAL = 3;

        public float updateTime;
        protected UdpClient udp;
        protected Timer heartbeatTimer;
        private Dictionary<byte, Action<byte, string, IPEndPoint>> handlerMap;

        public bool Active {
            get;
            protected set;
        }

        public EndPort() {
            this.handlerMap = new Dictionary<byte, Action<byte, string, IPEndPoint>>();
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

        public void RegisterHandler(byte id, Action<byte, string, IPEndPoint> Func) {
            this.handlerMap.Add(id, Func);
        }

        protected uint ToKCPClock() {
            return (uint)Mathf.FloorToInt(this.updateTime * 1000);
        } 

        protected void Handle(IPEndPoint ep, byte id, string data) {
            if (this.handlerMap.ContainsKey(id)) {
                this.handlerMap[id](id, data, ep);
            }
        }
        
        protected void Send(Connection connection, byte id, object msg=null) {
            byte[] buffer;

            if (msg != null) {
                var data = JsonConvert.SerializeObject(msg);
                buffer = new byte[Encoding.UTF8.GetByteCount(data) + 1];
                buffer[0] = id;
                Encoding.UTF8.GetBytes(data, 0, data.Length, buffer, 1);
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
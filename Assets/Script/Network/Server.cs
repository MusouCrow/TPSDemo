using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Network {
    public class Server : EndPort {
        private Dictionary<string, Connection> connectionMap;

        public int ConnectionCount {
            get {
                return this.connectionMap.Count;
            }
        }

        public Server() : base() {
            this.connectionMap = new Dictionary<string, Connection>();
            this.RegisterHandler(MsgId.Heartbeat, this.Heartbeat);
        }

        public bool Listen(int port) {
            if (base.Init()) {
                try {
                    this.udp = new UdpClient(port);
                }
                catch {
                    this.Active = false;
                    return false;
                }
                
                this.Receive();

                return true;
            }

            return false;
        }

        public override void Update(float dt) {
            if (!this.Active) {
                return;
            }

            this.updateTime += dt;
            this.heartbeatTimer.Update(dt);
            var clock = this.ToKCPClock();
            
            foreach (var c in this.connectionMap) {
                c.Value.Update(clock);
            }
        }

        public void Send(IPEndPoint ep, byte id, MessageBase message=null) {
            var fd = ep.ToString();

            if (this.connectionMap.ContainsKey(fd)) {
                var connection = this.connectionMap[fd];
                base.Send(connection, id, message);
            }
        }

        public void SendToAll(byte id, MessageBase message=null) {
            foreach (var c in this.connectionMap) {
                base.Send(c.Value, id, message);
            }
        }

        protected override void ReceiveCallback(IAsyncResult ar) {
            IPEndPoint ep = null;
            var buffer = this.udp.EndReceive(ar, ref ep);

            if (buffer != null) {
                string fd = ep.ToString();
                
                if (!this.connectionMap.ContainsKey(fd) && buffer[buffer.Length - 1] == MsgId.Connect) {
                    this.connectionMap.Add(fd, new Connection(ep, this.SendWrap, this.Handle));
                }

                if (this.connectionMap.ContainsKey(fd)) {
                    this.connectionMap[fd].Input(buffer);
                }
            }

            this.Receive();
        }

        protected override void HeartbeatTick() {
            var removeList = new List<string>();
            
            foreach (var c in this.connectionMap) {
                if (!c.Value.heartbeat) {
                    removeList.Add(c.Key);
                }
                else {
                    c.Value.heartbeat = false;
                }
            }

            foreach (var k in removeList) {
                this.Handle(this.connectionMap[k].EndPoint, MsgId.Disconnect, null);
                this.connectionMap.Remove(k);
            }

            this.heartbeatTimer.Enter();
        }

        private void Heartbeat(byte msgId, NetworkReader reader, IPEndPoint ep) {
            this.Send(ep, MsgId.Heartbeat);
            //Debug.Log("Server Heartbeat " + ep.ToString());
        }
    }
}

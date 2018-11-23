using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Network {
    public class Client : EndPort {
        private Connection connection;

        public bool Connect(string address, int port) {
            if (base.Init()) {
                this.udp = new UdpClient();

                var ipEndPoint = new IPEndPoint(Dns.GetHostAddresses(address)[0], port);
                this.connection = new Connection(ipEndPoint, this.SendWrap, this.Handle);
                
                this.Send(MsgId.Connect);
                this.Receive();

                return true;
            }

            return false;
        }

        public override bool Close() {
            if (base.Close()) {
                this.Handle(null, MsgId.Disconnect, null);

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
            this.connection.Update(this.ToKCPClock());
        }

        public void Send(byte id, MessageBase message=null) {
            base.Send(this.connection, id, message);
        }

        protected override void SendWrap(IPEndPoint ep, byte[] buffer, int size) {
            try {
                base.SendWrap(ep, buffer, size);
            }
            catch (SocketException) {
                this.Close();
            }
        }

        protected override void ReceiveCallback(IAsyncResult ar) {
            try {
                IPEndPoint ep = null;
                var buffer = this.udp.EndReceive(ar, ref ep);

                if (buffer != null) {
                    this.connection.Input(buffer);
                }

                this.Receive();
            }
            catch (SocketException) {
                this.Close();
            }
        }

        protected override void HeartbeatTick() {
            if (!this.connection.heartbeat) {
                this.Close();
            }
            else {
                this.Send(MsgId.Heartbeat);
                this.connection.heartbeat = false;
                this.heartbeatTimer.Enter();
            }
        }
    }
}

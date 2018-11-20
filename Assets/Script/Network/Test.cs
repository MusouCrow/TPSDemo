using System;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Network {
    public class Test : MonoBehaviour {
        [SerializeField]
        private string address;
        [SerializeField]
        private int port;

        private Server server;
        private Client client;

        protected void Start() {
            this.server = new Server();
            this.client = new Client();

            this.server.RegisterHandler(MsgId.Connect, this.NewConnection);
            this.server.RegisterHandler(MsgId.Disconnect, this.DelConnection);

            this.client.RegisterHandler(MsgId.Connect, this.ClientConnected);
            this.client.RegisterHandler(MsgId.Disconnect, this.ClientDisconnected);
            this.client.RegisterHandler(MsgId.Heartbeat, this.ClientHeartbeat);

            this.server.Listen(this.port);
            this.client.Connect(this.address, this.port);
        }

        protected void FixedUpdate() {
            var dt = Time.fixedDeltaTime;
            this.server.Update(dt);
            this.client.Update(dt);
        }

        private void NewConnection(byte msgId, NetworkReader reader, IPEndPoint ep) {
            print("New Client: " + ep.ToString());
            this.server.Send(ep, MsgId.Connect);
        }

        private void DelConnection(byte msgId, NetworkReader reader, IPEndPoint ep) {
            print("Del Client: " + ep.ToString());
        }

        private void ClientConnected(byte msgId, NetworkReader reader, IPEndPoint ep) {
            print("Client Connected");
        }

        private void ClientDisconnected(byte msgId, NetworkReader reader, IPEndPoint ep) {
            print("Client Disconnected");
        }

        private void ClientHeartbeat(byte msgId, NetworkReader reader, IPEndPoint ep) {
            print("Client Heartbeat");
        }
    }
}

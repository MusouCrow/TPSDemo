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
            this.client.RegisterHandler(MsgId.Connect, this.ClientConnected);

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

        private void ClientConnected(byte msgId, NetworkReader reader, IPEndPoint ep) {
            print("Client Connected");
        }
    }
}

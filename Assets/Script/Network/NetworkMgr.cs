using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

using Random = UnityEngine.Random;

namespace Game.Network {
    using Actor;
    using Utility;

    public class NetworkMgr : MonoBehaviour {
        public static event Action UpdateEvent;
        public static event Action LateUpdateEvent;
        public static bool localFirst = true;
        public const float STDDT = 0.017f;

        private const int DT = 17;
        private const int WAITTING_INTERVAL = 5;
        private static NetworkMgr INSTANCE;

        public static int Frame {
            get {
                return INSTANCE.frame;
            }
        }

        public static int PlayFrame {
            get {
                return INSTANCE.playFrame;
            }
        }

        public static string Addr {
            get {
                return INSTANCE.addr;
            }
        }

        public static void Send(string type, object data) {
            INSTANCE.sendList.Add(new InputData() {
                frame = INSTANCE.frame,
                type = type,
                data = data
            });
        }

        public static void Disconnect() {
            INSTANCE.client.Disconnect();
        }

        public static T DataToObject<T>(object data) {
            T obj;

            try {
                obj = (T)data;
            }
            catch {
                obj = JsonConvert.DeserializeObject<T>(data.ToString());
            }

            return obj;
        }

        [SerializeField]
        private string serverAddress;
        [SerializeField]
        private int serverPort;
        [SerializeField]
        private int version;
        private Client client;
        private string addr;
        private byte exitCode;
        private bool online;
        private int updateTimer;
        private int frame;
        private int playFrame;
        private List<InputData> sendList;
        private PlayData playData;

        protected void Awake() {
            INSTANCE = this;

            this.sendList = new List<InputData>();

            this.client = new Client(this.serverAddress, this.serverPort);
            this.client.RegisterEvent(EventCode.Connect, this.OnConnect);
            this.client.RegisterEvent(EventCode.Disconnect, this.OnDisconnect);
            this.client.RegisterEvent(EventCode.Start, this.OnStart);
            this.client.RegisterEvent(EventCode.Input, this.OnReceivePlayData);
        }

        protected void Start() {
            this.client.Connect();
        }

        protected void Update() {
            this.updateTimer += Mathf.CeilToInt(Time.deltaTime * 1000);

            while (this.updateTimer >= DT) {
                this.client.Update();
                this.LockUpdate();
                this.updateTimer -= DT;
            }

            if (this.exitCode != ExitCode.None) {
                this.addr = null;
                this.online = false;

                print("Disconnected");
                this.exitCode = ExitCode.None;
            }
        }
        
        private void LockUpdate() {
            if (this.online && this.frame + 1 == WAITTING_INTERVAL && this.playData == null) {
                return;
            }

            if (this.online) {
                this.frame++;

                if (this.frame == WAITTING_INTERVAL) {
                    var data = this.playData;
                    this.playData = null;
                    
                    if (data.addrs != null && data.inputs != null) {
                        for (int i = 0; i < data.addrs.Length; i++) {
                            if (!NetworkMgr.localFirst || (NetworkMgr.localFirst && this.addr != data.addrs[i])) {
                                ActorMgr.Input(data.addrs[i], data.inputs[i]);
                            }
                        }
                    }
                    
                    this.playFrame++;
                    this.frame = 0;

                    this.client.Send(EventCode.Input, new Datas.Input() {
                        inputs = this.sendList.ToArray(),
                        playFrame = this.playFrame
                    });

                    this.sendList.Clear();
                }

                NetworkMgr.UpdateEvent();
            }
        }

        private void OnConnect(byte id, string data) {
            var obj = JsonConvert.DeserializeObject<Datas.Connect>(data);

            if (obj.version != this.version || obj.isFull) {
                byte exitCode = obj.isFull ? ExitCode.Full : ExitCode.Version;
                this.client.Disconnect(exitCode);
            }
            else {
                this.client.Send(EventCode.Handshake, new Datas.Handshake() {
                    deviceModel = SystemInfo.deviceModel
                });
            }

            this.addr = obj.addr;
            print("Connected");
        }

        private void OnDisconnect(byte id, string data) {
            var obj = JsonConvert.DeserializeObject<Datas.Disconnect>(data);
            this.exitCode = obj.exitCode;
        }

        private void OnStart(byte id, string data) {
            var obj = JsonConvert.DeserializeObject<Datas.Start>(data);
            Random.InitState(obj.seed);

            var a = ActorMgr.NewShooter(obj.left.addr, obj.left.x, obj.left.y);
            var b = ActorMgr.NewShooter(obj.right.addr, obj.right.x, obj.right.y);
            var player = this.addr == obj.left.addr ? a : b;
            ActorMgr.BindCamera(player.transform);

            this.online = true;
            this.updateTimer = 0;
            this.frame = 0;
            this.playFrame = 0;
            this.exitCode = ExitCode.None;
            this.sendList.Clear();
            this.playData = new PlayData();
        }

        private void OnReceivePlayData(byte id, string data) {
            this.playData = JsonConvert.DeserializeObject<PlayData>(data);
        }
    }
}

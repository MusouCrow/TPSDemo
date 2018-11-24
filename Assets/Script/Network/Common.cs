using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Network {
    using Actor;

    public static class MsgId {
        public const byte Disconnect = 0;
        public const byte Connect = 1;
        public const byte Heartbeat = 2;
        public const byte NewPlayer = 3;
        public const byte DelPlayer = 4;
        public const byte Input = 5;
        public const byte Sync = 6;
    }
    
    namespace Msg {
        [Serializable]
        public class Connect {
            public string fd;
            public float updateTime;
            public PlayerData[] playerDatas;
        }

        [Serializable]
        public class DelPlayer {
            public string fd;
        }

        [Serializable]
        public class Input {
            public Snapshot[] snapshots;
        }

        [Serializable]
        public class Sync {
            public Snapshot[][] snapshotses;
        }
    }
}
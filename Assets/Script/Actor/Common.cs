using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Actor {
    using Utility;

    [Serializable]
    public class PlayerData {
        public string fd;
        public NVector3 position;
    }

    [Serializable]
    public class Snapshot {
        public string fd;
        public int frame;
        public string type;
        public object obj;
    }

    namespace Snapshots {
        [Serializable]
        public class Move {
            public NVector3 velocity;
            public NVector3 position; 
        }
    }
}
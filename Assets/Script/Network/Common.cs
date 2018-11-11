using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Network {
    [Serializable]
    public struct InputData {
        public int frame;
        public string type;
        public object data;
    }

    [Serializable]
    public class PlayData {
        public string[] addrs;
        public InputData[][] inputs;
        public int playFrame;
    }

    public static class EventCode {
        public const byte Disconnect = 0;
        public const byte Connect = 1;
        public const byte Heartbeat = 2;
        public const byte Start = 3;
        public const byte Input = 4;
        public const byte Comparison = 5;
        public const byte Handshake = 6;
    }

    public static class ExitCode {
        public const byte None = 0;
        public const byte Normal = 1;
        public const byte Full = 2;
        public const byte Version = 3;
    }

    namespace Datas {
        [Serializable]
        public struct Connect {
            public string addr;
            public int version;
            public bool isFull;
        }

        [Serializable]
        public struct Disconnect {
            public byte exitCode;
        }

        [Serializable]
        public struct Start {
            [Serializable]
            public struct Unit {
                public float x;
                public float y;
                public string addr;
            }

            public int seed;
            public Unit left;
            public Unit right;
        }

        [Serializable]
        public struct Comparison {
            public int playFrame;
            public string content;
        }

        [Serializable]
        public struct Handshake {
            public string deviceModel;
        }

        [Serializable]
        public struct Input {
            public InputData[] inputs;
            public int playFrame;
        }

        [Serializable]
        public struct Move {
            public float x;
            public float z; 
        }

        public struct Rotate {
            public float value;
        }
    }
}
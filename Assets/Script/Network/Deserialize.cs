using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Network {
    using Actor;

    public class Deserialize : MonoBehaviour {
        public TextAsset file;

        protected void Start() {
            var reader = new NetworkReader(file.bytes);
            var msg = new Msg.Sync() {syncList = new List<List<Snapshot>>()};
            msg.Deserialize(reader);
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Actor {
    using Network;

    public class Identity : LockBehaviour {
        [NonSerialized]
        public string addr;

        private int inputIndex;
        private InputData[] inputDatas;
        private Dictionary<string, Action<object>> eventMap = new Dictionary<string, Action<object>>();

        public bool IsPlayer {
            get {
                return NetworkMgr.Addr == this.addr;
            }
        }

        protected override void LockUpdate() {
            while (this.inputDatas != null && this.inputDatas.Length > this.inputIndex && this.inputDatas[this.inputIndex].frame == NetworkMgr.Frame) {
                var inputData = this.inputDatas[this.inputIndex];

                if (this.eventMap.ContainsKey(inputData.type)) {
                    this.eventMap[inputData.type](inputData.data);
                }

                this.inputIndex++;
            }
        }

        public void AddEvent(string type, Action<object> func) {
            this.eventMap.Add(type, func);
        }

        public void SetInputDatas(InputData[] datas) {
            this.inputIndex = 0;
            this.inputDatas = datas;
        }

        public void Send(string type, object data) {
            NetworkMgr.Send(type, data);
            
            if (NetworkMgr.localFirst && this.eventMap.ContainsKey(type)) {
                this.eventMap[type](data);
            }
        }
    }
}

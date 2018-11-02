using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Network {
    using PlayData = InputData;

    public class Network : MonoBehaviour {
        public const float STDDT = 0.017f;
        public static event Action UpdateEvent;
        public static event Action LateUpdateEvent;
        public const int WAITTING_INTERVAL = 5;
        public Shooter.Input input;
        
        private const int DT = 17;
        private int updateTimer;
        private int frame;
        private int playFrame;
        private List<PlayData> playDataList;
        private List<PlayData> recvList;
        private bool sendInLoop;

        protected void Awake() {
            this.playDataList = new List<PlayData>();
            this.recvList = new List<PlayData>();

            this.recvList.Add(new PlayData());
        }

        protected void Update() {
            this.updateTimer += Mathf.CeilToInt(Time.deltaTime * 1000);

            while (this.updateTimer >= DT) {
                this.Recv();

                if (this.playDataList.Count > 1) {
                    var lateFrame = this.frame;
                    this.sendInLoop = true;

                    do {
                        this.Recv();
                        this.LockUpdate(true);
                    } while(this.playDataList.Count == 1 && this.frame == lateFrame);
                }

                this.LockUpdate();
                this.updateTimer -= DT;
            }
        }

        private void LockUpdate(bool inLoop=false) {
            if (this.frame + 1 == WAITTING_INTERVAL && this.playDataList.Count == 0) {
                return;
            }

            this.frame++;

            if (this.frame == WAITTING_INTERVAL) {
                var data = this.playDataList[0];
                this.playDataList.RemoveAt(0);
                this.input.SetData(data);

                this.playFrame++;
                this.frame = 0;

                if (!inLoop || (inLoop && this.sendInLoop)) {
                    this.recvList.Add(new PlayData() {
                        vertical = Input.GetAxis("Vertical"),
                        horizontal = Input.GetAxis("Horizontal"),
                        mouseX = Input.GetAxis("Mouse X"),
                        fire = Input.GetKey(KeyCode.Mouse0)
                    });

                    this.sendInLoop = false;
                }
            }
            
            Network.UpdateEvent();
            //Network.LateUpdateEvent();
        }

        private void Recv() {
            if (this.recvList.Count > 0) {
                var data = this.recvList[0];
                this.recvList.RemoveAt(0);
                this.playDataList.Add(data);
            }
        }
    }
}

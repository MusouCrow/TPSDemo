using System;
using UnityEngine;

namespace Game.Actor {
    using Network;
    using Snapshots;

    public class Battle : MonoBehaviour {
        [NonSerialized]
        public int hp;
        public int maxHp;

        private Identity identity;
        private Shaking shaking;

        protected void Awake() {
            this.hp = this.maxHp;
        }

        protected void Start() {
            this.identity = this.GetComponent<Identity>();
            this.identity.BindEvent(typeof(Damage), this.Damage);

            this.shaking = this.GetComponent<Shaking>();
        }

        public void Beaten() {
            this.shaking.Shake(0.5f, 0.1f);
            
            if (ServerMgr.Active) {
                this.hp -= 1;
                this.hp = this.hp <= 0 ? this.maxHp : this.hp;
                this.identity.ServerInput(new Damage() {hp = this.hp});
            }
        }

        public void HandlePlayerData(PlayerData playerData) {
            playerData.hp = this.hp;
        }

        private void Damage(Snapshot snapshot) {
            var damage = snapshot as Snapshots.Damage;
            this.hp = damage.hp;
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Actor {
    public class HpBar : MonoBehaviour {
        public Battle battle;
        public Text text;

        private int hp;
        private new RectTransform transform;

        protected void Start() {
            this.transform = this.GetComponent<RectTransform>();
            
            var name = this.battle.gameObject.name;
            int pos = name.IndexOf(':');
            this.text.text = name.Substring(pos + 1);
            this.FixedUpdate();
        }

        protected void FixedUpdate() {
            if (this.hp != this.battle.hp) {
                this.hp = this.battle.hp;
                this.transform.sizeDelta = new Vector2((float)this.battle.hp / (float)this.battle.maxHp, this.transform.sizeDelta.y);
            }
        }
    }
}
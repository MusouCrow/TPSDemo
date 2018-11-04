using UnityEngine;

namespace Game.Network {
    public class LockBehaviour : MonoBehaviour {
        public enum OrderType {
            Normal,
            Late
        }

        [SerializeField]
        protected OrderType orderType = OrderType.Normal;

        protected void Awake() {
            if (this.orderType == OrderType.Normal) {
                NetworkMgr.UpdateEvent += this.LockUpdateWrap;
            }
            else {
                NetworkMgr.LateUpdateEvent += this.LockUpdateWrap;
            }
        }
        
        protected void OnDestroy() {
            if (this.orderType == OrderType.Normal) {
                NetworkMgr.UpdateEvent -= this.LockUpdateWrap;
            }
            else {
                NetworkMgr.LateUpdateEvent -= this.LockUpdateWrap;
            }
        }

        private void LockUpdateWrap() {
            if (this.isActiveAndEnabled) {
                this.LockUpdate();
            }
        }
        
        protected virtual void LockUpdate() {}
    }
}
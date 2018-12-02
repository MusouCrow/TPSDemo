using UnityEngine;
using DG.Tweening;

namespace Game.Actor {
    public class Shaking : MonoBehaviour {
        private Sequence sequence;

        public void Shake(float duration, float strength) {
            if (this.sequence != null) {
                this.sequence.Kill();
            }

            this.sequence = DOTween.Sequence();
            this.sequence.Append(this.transform.DOShakeScale(duration, strength));
            this.sequence.Append(this.transform.DOScale(Vector3.one, 0.1f));
        }
    }
}
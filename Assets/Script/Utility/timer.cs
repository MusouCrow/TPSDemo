using System;

namespace Game.Utility {
	public class Timer {
		public bool IsRunning {
			get;
            private set;
		}

		public float value;
		public float max;
		private Action OnComplete;

		public Timer(float time=0, Action OnComplete=null) {
			this.value = 0;
			this.max = 0;
			this.IsRunning = false;

			if (time > 0) {
				this.Enter(time, OnComplete);
			}
		}

		public void Update(float dt) {
			if (!this.IsRunning) {
				return;
			}

			this.value += dt;

			if (this.value >= this.max) {
				this.value = this.max;
				this.Exit();
				this.OnComplete();
			}
		}

		public void Enter(float time=0, Action OnComplete=null) {
			this.value = 0;
			this.IsRunning = true;
			this.max = time == 0 ? this.max : time;
			this.OnComplete = OnComplete == null ? this.OnComplete : OnComplete;
		}

		public void Exit() {
			this.IsRunning = false;
		}
	}
}

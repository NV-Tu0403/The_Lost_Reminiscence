using UnityEngine;

namespace Code.UI.Gameplay.HeartBeat
{
    public class HeartRateStateManager : MonoBehaviour
    {
        [SerializeField] private SWP_HeartRateMonitor heartRateMonitor;

        public enum HeartState
        {
            Normal,
            FastScared,
            SlowWeak,
            Unconscious,
            FastThenNormal
        }

        private HeartState currentState = HeartState.Normal;
        private float transitionTimer = 0f;
        private float transitionDuration = 5f; // Thời gian chuyển từ nhanh về bình thường

        private void Start()
        {
            if (heartRateMonitor == null)
            {
                heartRateMonitor = GetComponent<SWP_HeartRateMonitor>();
                if (heartRateMonitor == null)
                {
                    Debug.LogError("HeartRateMonitor component not found!");
                }
            }
            ApplyState(HeartState.Normal); // Bắt đầu với trạng thái bình thường
        }

        private void Update()
        {
            // Xử lý trạng thái FastThenNormal
            if (currentState == HeartState.FastThenNormal)
            {
                transitionTimer += Time.deltaTime;
                if (transitionTimer >= transitionDuration)
                {
                    // Không tự động chuyển, chờ script khác gọi
                    transitionTimer = 0f;
                }
            }
        }

        // Áp dụng trạng thái mới
        public void ApplyState(HeartState state)
        {
            currentState = state;
            switch (state)
            {
                case HeartState.Normal:
                    heartRateMonitor.BeatsPerMinute = 90;
                    heartRateMonitor.FlatLine = false;
                    heartRateMonitor.SetHeartRateColour(heartRateMonitor.NormalColour);
                    break;

                case HeartState.FastScared:
                    heartRateMonitor.BeatsPerMinute = 140;
                    heartRateMonitor.FlatLine = false;
                    heartRateMonitor.SetHeartRateColour(heartRateMonitor.MediumColour);
                    break;

                case HeartState.SlowWeak:
                    heartRateMonitor.BeatsPerMinute = 40;
                    heartRateMonitor.FlatLine = false;
                    heartRateMonitor.SetHeartRateColour(heartRateMonitor.BadColour);
                    break;

                case HeartState.Unconscious:
                    heartRateMonitor.BeatsPerMinute = 0;
                    heartRateMonitor.FlatLine = true;
                    heartRateMonitor.SetHeartRateColour(heartRateMonitor.FlatlineColour);
                    break;

                case HeartState.FastThenNormal:
                    heartRateMonitor.BeatsPerMinute = 140;
                    heartRateMonitor.FlatLine = false;
                    heartRateMonitor.SetHeartRateColour(heartRateMonitor.MediumColour);
                    transitionTimer = 0f; // Bắt đầu đếm nhưng không tự động chuyển
                    break;
            }
        }

        // Phương thức kiểm tra trạng thái hiện tại (nếu cần)
        public HeartState GetCurrentState()
        {
            return currentState;
        }
    }
}

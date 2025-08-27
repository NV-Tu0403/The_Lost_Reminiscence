using _ThirdParty.SWP_HeartRateMonitor.Scripts;
using UnityEngine;

namespace Code.UI.Gameplay.HeartBeat
{
    public class HeartRateStateManager : MonoBehaviour
    {
        [SerializeField] private SwpHeartRateMonitor heartRateMonitor;

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
                heartRateMonitor = GetComponent<SwpHeartRateMonitor>();
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
                    heartRateMonitor.beatsPerMinute = 90;
                    heartRateMonitor.flatLine = false;
                    heartRateMonitor.SetHeartRateColour(heartRateMonitor.normalColour);
                    break;

                case HeartState.FastScared:
                    heartRateMonitor.beatsPerMinute = 140;
                    heartRateMonitor.flatLine = false;
                    heartRateMonitor.SetHeartRateColour(heartRateMonitor.mediumColour);
                    break;

                case HeartState.SlowWeak:
                    heartRateMonitor.beatsPerMinute = 40;
                    heartRateMonitor.flatLine = false;
                    heartRateMonitor.SetHeartRateColour(heartRateMonitor.badColour);
                    break;

                case HeartState.Unconscious:
                    heartRateMonitor.beatsPerMinute = 0;
                    heartRateMonitor.flatLine = true;
                    heartRateMonitor.SetHeartRateColour(heartRateMonitor.flatlineColour);
                    break;

                case HeartState.FastThenNormal:
                    heartRateMonitor.beatsPerMinute = 140;
                    heartRateMonitor.flatLine = false;
                    heartRateMonitor.SetHeartRateColour(heartRateMonitor.mediumColour);
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

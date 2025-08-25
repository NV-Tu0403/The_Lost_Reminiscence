using System;
using TMPro;
using UnityEngine;

namespace _MyGame.Codes.Dialogue
{
    public class AutoResizeBackground : MonoBehaviour
    {
        [Header("Refs")]
        public RectTransform container;           // Image background (ô vuông)
        public TextMeshProUGUI text;              // Text TMP bên trong

        [Header("Square Settings")]
        public float minSide = 280f;
        public float maxSide = 520f;
        public Vector2 padding = new Vector2(40f, 40f); // L/R, T/B

        [Header("Text Fit Mode")]
        public bool useAutoSize = false;          // true = co chữ; false = dùng Ellipsis
        public float autoSizeMin = 18f;
        public float autoSizeMax = 32f;

        [Obsolete("Obsolete")]
        private void LateUpdate()
        {
            if (!container || !text) return;

            // Chọn thử một cạnh mong muốn (bạn có thể set sẵn bằng maxSide để luôn là ô vuông max)
            var side = Mathf.Clamp(maxSide, minSide, maxSide);

            // Đặt container thành ô vuông
            SetSquare(container, side);

            // Kích thước vùng text trừ padding
            var textW = Mathf.Max(1f, side - padding.x);
            var textH = Mathf.Max(1f, side - padding.y);
            text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textW);
            text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,  textH);

            // Cấu hình wrap/overflow
            text.enableWordWrapping = true;

            if (useAutoSize)
            {
                text.enableAutoSizing = true;
                text.fontSizeMin = autoSizeMin;
                text.fontSizeMax = autoSizeMax;
                text.overflowMode = TextOverflowModes.Overflow; // cho phép co chữ thay vì cắt
            }
            else
            {
                text.enableAutoSizing = false;
                text.overflowMode = TextOverflowModes.Ellipsis; // “…” nếu vẫn dư
            }

            // Cập nhật mesh để lấy chiều cao thực
            text.ForceMeshUpdate();
        }

        private static void SetSquare(RectTransform rt, float side)
        {
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, side);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   side);
        }
    }
}

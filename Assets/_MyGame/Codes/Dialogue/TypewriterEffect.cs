using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Code.Dialogue
{
    public static class TypewriterEffect
    {
        /// <summary>
        /// Hiệu ứng typewriter cho LocalizedString:
        /// - Lấy text từ LocalizedString (chờ load xong)
        /// - Gõ từng ký tự lên TextMeshProUGUI với delay
        /// </summary>
        public static IEnumerator PlayLocalized(TextMeshProUGUI textComponent, LocalizedString locStr, float delay)
        {
            string result = null;
            var received = false;
            locStr.StringChanged += (localizedText) =>
            {
                result = localizedText;
                received = true;
            };
            locStr.RefreshString();
            while (!received)
                yield return null;
            yield return Play(textComponent, result, delay);
        }
        
        /// <summary>
        /// Hiệu ứng typewriter cho string thường:
        /// - Gõ từng ký tự lên TextMeshProUGUI với delay
        /// </summary>
        private static IEnumerator Play(TextMeshProUGUI textComponent, string fullText, float delay)
        {
            if (textComponent == null || string.IsNullOrEmpty(fullText))
                yield break;
            textComponent.text = "";
            for (var i = 0; i < fullText.Length; i++)
            {
                if (i + 1 >= 0 && i + 1 <= fullText.Length) textComponent.text = fullText[..(i + 1)];
                yield return new WaitForSeconds(delay);
            }
            textComponent.text = fullText;
        }
    }
}
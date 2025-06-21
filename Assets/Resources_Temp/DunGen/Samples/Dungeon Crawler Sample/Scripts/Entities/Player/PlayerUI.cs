using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DunGen.DungeonCrawler
{
	/// <summary>
	/// Controls the basic player UI
	/// </summary>
	sealed class PlayerUI : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("Text box to display the player's current gold")]
		private Text goldText = null;

		[SerializeField]
		[Tooltip("An image that is only visible when the player has the key")]
		private Image keyIcon = null;

		[SerializeField]
		[Tooltip("Text box for displaying messages to the player")]
		private Text messageText = null;

		[SerializeField]
		[Tooltip("Used to fade the message box in/out")]
		private CanvasGroup messageGroup = null;

		[SerializeField]
		[Tooltip("A curve of the message's opacity over time")]
		private AnimationCurve messageOpacityCurve = null;

		[SerializeField]
		[Tooltip("Used to toggle visibilty of on-screen instructions")]
		private CanvasGroup instructionsGroup = null;

		private ObjectCollector collector;
		private Coroutine messageFadeCoroutine;


		private void Start()
		{
			messageGroup.alpha = 0f;
		}

		private void OnDestroy()
		{
			ClearPlayer();
		}

		private void Update()
		{
			// Toggle on-screen instructions
			if (Input.GetKeyDown(KeyCode.F1))
				instructionsGroup.alpha = (instructionsGroup.alpha == 1f) ? 0f : 1f;
		}

		/// <summary>
		/// Tell the UI which player it belongs to
		/// </summary>
		public void SetPlayer(GameObject newPlayer)
		{
			if (collector != null)
				ClearPlayer();

			collector = newPlayer.GetComponent<ObjectCollector>();
			collector.GoldChanged += RefreshGoldCounter;
			collector.KeyCollectionChanged += RefreshKeyIcon;

			RefreshGoldCounter();
			RefreshKeyIcon();
		}

		/// <summary>
		/// Clears the existing player and cleans up any event listeners
		/// </summary>
		private void ClearPlayer()
		{
			collector.GoldChanged -= RefreshGoldCounter;
			collector.KeyCollectionChanged -= RefreshKeyIcon;

			collector = null;
		}

		private void RefreshGoldCounter()
		{
			goldText.text = string.Format("Gold: {0:N0}", collector.Gold);
		}

		private void RefreshKeyIcon()
		{
			// We only have one key type in the sample, but we could easily
			// check for multiple keys and enable/disable different icons
			keyIcon.enabled = collector.HasKey(0);
		}

		public void ShowMessage(string message)
		{
			if (messageFadeCoroutine != null)
				StopCoroutine(messageFadeCoroutine);

			messageFadeCoroutine = StartCoroutine(ShowMessageCoroutine(message));
		}

		private IEnumerator ShowMessageCoroutine(string message)
		{
			messageGroup.alpha = 0f;
			messageText.text = message;

			float duration = messageOpacityCurve.keys.Last().time;
			float time = 0f;

			while (time < duration)
			{
				yield return null;
				time = Mathf.Min(time + Time.deltaTime, duration);

				float opacity = messageOpacityCurve.Evaluate(time);
				messageGroup.alpha = opacity;
			}

			messageFadeCoroutine = null;
		}
	}
}

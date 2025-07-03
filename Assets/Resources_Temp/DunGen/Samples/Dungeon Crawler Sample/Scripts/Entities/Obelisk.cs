using UnityEngine;

namespace DunGen.DungeonCrawler
{
	/// <summary>
	/// A clickable shrine that gives the player a buff
	/// </summary>
	sealed class Obelisk : MonoBehaviour, IClickableObject
	{
		[SerializeField]
		[Tooltip("The cursor to use when the player hovers over it with the mouse")]
		private Texture2D hoverCursor = null;

		[SerializeField]
		[Tooltip("How close the player has to be to interact. If further away, the player will run to the obelisk first")]
		private float interactDistance = 5f;

		[SerializeField]
		[Tooltip("An image that's rendered to the minimap")]
		private GameObject minimapIcon = null;

		[SerializeField]
		[Tooltip("The value to multiply the players speed by when interacted with")]
		private float speedMultiplier = 1.25f;

		[SerializeField]
		[Tooltip("A particle effect to spawn when the player interacts with the obelisk")]
		private GameObject particlesPrefab = null;

		/// <summary>
		/// Has this obelisk already been used?
		/// </summary>
		private bool depleted;


		public bool CanInteract()
		{
			// Only allow the player to use the chest if it hasn't already been opened
			return !depleted;
		}

		public Texture2D GetHoverCursor()
		{
			return hoverCursor;
		}

		public void Interact()
		{
			var player = FindObjectOfType<ClickToMove>();
			float distanceToPlayer = (transform.position - player.transform.position).magnitude;

			// If we're in range to use the obelisk, just do it...
			if (distanceToPlayer <= interactDistance)
				Use(player);
			// ...otherwise, have the player run to the obelisk instead
			else
				PathTo(player);
		}

		/// <summary>
		/// Have the player move to the obelisk's location
		/// </summary>
		/// <param name="player">The player's ClickToMove component</param>
		private void PathTo(ClickToMove player)
		{
			// Tell the player to move to a point in front of the obelisk
			Vector3 destination = transform.position + transform.forward * 0.5f;

			player.StopManualMovement();
			player.MoveTo(destination, true);
		}

		public void Use(ClickToMove player)
		{
			if (depleted)
				return;

			depleted = true;

			if (minimapIcon != null)
				minimapIcon.SetActive(false);

			player.Agent.speed *= speedMultiplier;

			var playerUI = FindObjectOfType<PlayerUI>();
			playerUI.ShowMessage("Movement Speed Increased");

			if (particlesPrefab != null)
			{
				var particles = Instantiate(particlesPrefab, player.transform);
				particles.transform.localPosition = Vector3.zero;
			}
		}
	}
}

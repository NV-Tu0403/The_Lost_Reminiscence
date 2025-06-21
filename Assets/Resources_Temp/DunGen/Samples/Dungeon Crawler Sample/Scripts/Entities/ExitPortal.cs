using UnityEngine;

namespace DunGen.DungeonCrawler
{
	/// <summary>
	/// The exit portal that generates a new dungeon when clicked
	/// </summary>
	sealed class ExitPortal : MonoBehaviour, IClickableObject
	{
		[SerializeField]
		[Tooltip("The cursor to use when the player hovers over it with the mouse")]
		private Texture2D hoverCursor = null;

		[SerializeField]
		[Tooltip("How close the player has to be to interact. If further away, the player will run to the portal first")]
		private float interactDistance = 5f;


		public bool CanInteract()
		{
			return true;
		}

		public Texture2D GetHoverCursor()
		{
			return hoverCursor;
		}

		public void Interact()
		{
			var player = FindObjectOfType<ClickToMove>();
			float distanceToPlayer = (transform.position - player.transform.position).magnitude;

			// If we're in range to use the portal, just do it...
			if (distanceToPlayer <= interactDistance)
				Use(player);
			// ...otherwise, have the player run to the portal instead
			else
				PathTo(player);
		}

		/// <summary>
		/// Have the player move to the portal's location
		/// </summary>
		/// <param name="player">The player's ClickToMove component</param>
		private void PathTo(ClickToMove player)
		{
			// Tell the player to move to a point in front of the portal
			Vector3 destination = transform.position + transform.forward * 0.5f;

			player.StopManualMovement();
			player.MoveTo(destination, true);
		}

		public void Use(ClickToMove player)
		{
			// Reset the cursor so it's not permanently set to the portal cursor
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

			var runtimeDungeon = FindObjectOfType<RuntimeDungeon>();
			runtimeDungeon.Generate();
		}
	}
}

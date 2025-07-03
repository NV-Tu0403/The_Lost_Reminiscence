using UnityEngine;

namespace DunGen.DungeonCrawler
{
	/// <summary>
	/// An interfac for all clickable objects (treasure chests, shrines, etc)
	/// Actual logic is handled by <see cref="ClickableObjectHandler"/>
	/// </summary>
	interface IClickableObject
	{
		Texture2D GetHoverCursor();
		bool CanInteract();
		void Interact();
	}
}

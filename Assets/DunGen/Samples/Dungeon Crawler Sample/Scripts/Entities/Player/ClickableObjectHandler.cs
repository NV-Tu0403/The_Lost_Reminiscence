using System.Linq;
using UnityEngine;

namespace DunGen.DungeonCrawler
{
	/// <summary>
	/// Handles casting rays to determine if the mouse cursor is hovering over any clickable objects.
	/// Controls the custom cursor displayed when mousing over a clickable object as well as handling
	/// the click action. We don't just use Unity's OnMouseHover event because we need control over which
	/// layers are tested with the raycast (so faded walls don't block our clicks).
	/// </summary>
	sealed class ClickableObjectHandler : MonoBehaviour
	{
		public IClickableObject HoverClickable { get; private set; }

		private void RefreshCursor()
		{
			var cursor = (HoverClickable != null) ? HoverClickable.GetHoverCursor() : null;
			Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
		}

		[SerializeField]
		[Tooltip("Which layers should be checked during the raycast")]
		private LayerMask raycastLayer = -1;

		[SerializeField]
		[Tooltip("How far the raycast travels when checking for hover/click")]
		private float maxRayDistance = 100f;

		[SerializeField]
		[Tooltip("Which camera to cast the cursor rays from")]
		private Camera raycastCamera = null;

		private RaycastHit[] hitBuffer = new RaycastHit[8];


		private void Update()
		{
			var previousHoverObject = HoverClickable;
			HoverClickable = GetClickableUnderCursor();

			if (previousHoverObject != HoverClickable)
				RefreshCursor();
		}

		public void Click()
		{
			if (HoverClickable == null || !HoverClickable.CanInteract())
				return;

			HoverClickable.Interact();
		}

		private IClickableObject GetClickableUnderCursor()
		{
			var ray = raycastCamera.ScreenPointToRay(Input.mousePosition);
			int hitCount = Physics.RaycastNonAlloc(ray, hitBuffer, maxRayDistance, raycastLayer, QueryTriggerInteraction.Ignore);

			// This should probably be replaced in production code.
			// LINQ might not be performant enough, but it's the easiest way to  get a distance-ordered list of hit results
			var hits = hitBuffer.Take(hitCount).OrderBy(x => x.distance);

			foreach (var hit in hits)
			{
				var clickable = hit.collider.GetComponent<IClickableObject>();

				if (clickable != null && clickable.CanInteract())
					return clickable;
			}

			return null;
		}
	}
}

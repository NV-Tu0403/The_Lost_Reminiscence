using System.Collections.Generic;
using UnityEngine;

namespace DunGen.DungeonCrawler
{
	/// <summary>
	/// Handles hiding/showing objects containing a <see cref="HideableObject"/> component
	/// by casting a sphere from the camera to the player
	/// </summary>
	sealed class ObjectHidingCamera : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("The object to check line of sight to")]
		private Transform target = null;

		[SerializeField]
		[Tooltip("The radius of the sphere cast used to check for occluding objects")]
		private float sphereCastRadius = 2f;


		private RaycastHit[] hitBuffer = new RaycastHit[32];
		private List<HideableObject> hiddenObjects = new List<HideableObject>();
		private List<HideableObject> previouslyHiddenObjects = new List<HideableObject>();


		private void LateUpdate()
		{
			RefreshHiddenObjects();
		}

		public void RefreshHiddenObjects()
		{
			// Calculate ray to target transform
			Vector3 toTarget = (target.position - transform.position);
			float targetDistance = toTarget.magnitude;
			Vector3 targetDirection = toTarget / targetDistance;

			// Stop short of the target so we don't accidently hit a wall behind the player
			targetDistance -= sphereCastRadius * 1.1f;

			hiddenObjects.Clear();

			int hitCount = Physics.SphereCastNonAlloc(transform.position, sphereCastRadius, targetDirection, hitBuffer, targetDistance, -1, QueryTriggerInteraction.Ignore);

			// Gather hideable objects this frame
			for (int i = 0; i < hitCount; i++)
			{
				var hit = hitBuffer[i];
				var hideable = HideableObject.GetRootHideableByCollider(hit.collider);

				if (hideable != null)
					hiddenObjects.Add(hideable);
			}

			// Newly hidden
			foreach (var hideable in hiddenObjects)
				if (!previouslyHiddenObjects.Contains(hideable))
					hideable.SetVisible(false);

			// Newly visible
			foreach (var hideable in previouslyHiddenObjects)
				if (!hiddenObjects.Contains(hideable))
					hideable.SetVisible(true);

			// Swap lists
			var temp = hiddenObjects;
			hiddenObjects = previouslyHiddenObjects;
			previouslyHiddenObjects = temp;
		}
	}
}

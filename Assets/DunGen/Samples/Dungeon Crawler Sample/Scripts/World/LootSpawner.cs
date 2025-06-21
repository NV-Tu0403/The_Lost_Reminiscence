using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DunGen.DungeonCrawler
{
	/// <summary>
	/// Spawns a fountain of loot in the nearby area.
	/// Spawned objects are constrained to the walkable area of the NavMesh.
	/// </summary>
	sealed class LootSpawner : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("The radius around this object that loot should be spawned")]
		private float range = 3f;

		[SerializeField]
		[Tooltip("The delay between objects being spawned")]
		private float interval = 0.5f;

		[SerializeField]
		[Tooltip("How long (in seconds) it takes to complete the parabola animation")]
		private float animationDuration = 1f;

		[SerializeField]
		[Tooltip("The minimum height the spawned loot will reach")]
		private float minParabolaHeight = 1.5f;

		[SerializeField]
		[Tooltip("The maximum height the spawned loot will reach")]
		private float maxParabolaHeight = 3f;


		/// <summary>
		/// Release a stream of loot instances over time
		/// </summary>
		/// <param name="objects">List of objects to release</param>
		public void Release(IEnumerable<GameObject> objects)
		{
			// Make sure the objects are all inactive initially
			foreach (var obj in objects)
				if (obj.activeSelf)
					obj.SetActive(false);

			StartCoroutine(ReleaseCoroutine(objects));
		}

		private IEnumerator ReleaseCoroutine(IEnumerable<GameObject> objects)
		{
			foreach (var obj in objects)
			{
				StartCoroutine(ReleaseSingleCoroutine(obj));
				yield return new WaitForSeconds(interval);
			}
		}

		/// <summary>
		/// Attempt to find a random spawn point within range
		/// </summary>
		/// <param name="origin">The center of the spawn</param>
		/// <param name="closestPointOnNavMesh">Closest point on the navigation mesh</param>
		/// <param name="destination">Output destination, or the origin if no spawn point could be found</param>
		/// <returns>True if the spawn point is valid</returns>
		private bool TryGetValidSpawnPoint(Vector3 origin, Vector3 closestPointOnNavMesh, out Vector3 destination)
		{
			// There's no guarantee we'll be able to find a valid spawn point
			// Try 20 times to find a valid spawn point, then give up
			const int maxTryCount = 20;
			for (int i = 0; i < maxTryCount; i++)
			{
				destination = origin + Random.insideUnitSphere * range;

				// Find the closest point on the navmesh
				NavMeshHit hit;
				if (NavMesh.SamplePosition(destination, out hit, range, -1))
				{
					destination = hit.position;

					// Calculate a path to the destination
					// NavMesh.SamplePosition() returns the closest point on the navmesh, but doesn't account for
					// blocking geometry. To avoid spawning loot on the wrong side of a nearby wall, we calculate
					// a path to the desired spawn point, then calculate the length of the path. If the path is longer
					// than the loot spawn radius, it was spawned behind a wall and should be discarded.
					var path = new NavMeshPath();
					if (NavMesh.CalculatePath(closestPointOnNavMesh, destination, -1, path) &&
						path.status == NavMeshPathStatus.PathComplete &&
						NavMeshUtil.CalculatePathLength(path) <= range)
					{
						return true;
					}
				}
			}

			destination = origin;
			return false;
		}

		/// <summary>
		/// Coroutine for animating a single object instance
		/// </summary>
		/// <param name="obj">The object to animate</param>
		private IEnumerator ReleaseSingleCoroutine(GameObject obj)
		{
			Vector3 origin = transform.position;
			Vector3 destination;
			NavMeshHit closestPointHit;

			// Find the closest point on the navmesh to this spawner, and use it to try to find a valid spawn point for the loot
			NavMesh.SamplePosition(origin, out closestPointHit, range, -1);
			TryGetValidSpawnPoint(origin, closestPointHit.position, out destination);

			obj.SetActive(true);
			var collectible = obj.GetComponent<ICollectibleObject>();

			// We don't want the player to be able to pick it up mid-flight
			if (collectible != null)
				collectible.CanPickUp = false;

			float time = 0f;
			float maxHeight = Random.Range(minParabolaHeight, maxParabolaHeight);

			while (time < animationDuration)
			{
				time += Time.deltaTime;
				yield return null;

				// Normalized (0-1) animation length
				float alpha = Mathf.Clamp01(time / animationDuration);

				Vector3 position = Vector3.Lerp(transform.position, destination, alpha);

				// Use a parabola equation to calculate the height at a given point along the animation
				float height = (1 - Mathf.Pow((2 * alpha - 1), 2)) * maxHeight;
				position += Vector3.up * height;

				obj.transform.position = position;
			}

			// Allow the player to pick it up once it has landed on the ground
			if (collectible != null)
				collectible.CanPickUp = true;
		}
	}
}

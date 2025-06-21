using System;
using UnityEngine;
using UnityEngine.AI;

namespace DunGen.DungeonCrawler
{
	/// <summary>
	/// Handles player movement. Requires an attached NavMesh agent.
	/// Input is handled in <see cref="PlayerInput"/>
	/// </summary>
	[RequireComponent(typeof(NavMeshAgent))]
	sealed class ClickToMove : MonoBehaviour
	{
		public NavMeshAgent Agent { get; private set; }

		[SerializeField]
		[Tooltip("An object spawned at the click location by the player")]
		private GameObject pingPrefab = null;

		[SerializeField]
		[Tooltip("Which layers are tested by the click raycast")]
		private LayerMask rayLayerMask = -1;

		/// <summary>
		/// An event fired when <see cref="MoveTo(Vector3)"/> is called
		/// </summary>
		public event Action BeginMove;

		private Vector3 manualMovementDirection;
		private NavMeshPath path;


		private void Awake()
		{
			Agent = GetComponent<NavMeshAgent>();
			path = new NavMeshPath();
		}

		public void Click(Ray ray, bool pingLocation)
		{
			RaycastHit hit;
			const float maxDistance = 100f;

			if (Physics.Raycast(ray, out hit, maxDistance, rayLayerMask, QueryTriggerInteraction.Ignore))
			{
				Vector3 destination = MoveTo(hit.point, false);

				if (pingLocation)
					Instantiate(pingPrefab, destination, Quaternion.identity);
			}
		}

		public Vector3 MoveTo(Vector3 destination, bool forceCalculatePath)
		{
			NavMeshHit hit;

			// Project the destination onto the NavMesh
			if (NavMesh.SamplePosition(destination, out hit, 2f, -1))
				destination = hit.position;


			// If the destination is close enough and a raycast through the navmesh hits no
			// edges, there's no obstructions and we can manually move the player without
			// requesting a path from the NavMesh system. We do this to avoid stuttering
			// when the player clicks at a position to close to the character
			bool requiresPath;

			if (forceCalculatePath)
				requiresPath = true;
			else
			{
				const float manualMoveDistanceThreshold = 1.5f;
				float distanceToDestination = (transform.position - destination).magnitude;

				if (distanceToDestination > manualMoveDistanceThreshold)
					requiresPath = true;
				else
					requiresPath = NavMesh.Raycast(transform.position, destination, out hit, -1);
			}

			if (!requiresPath)
			{
				manualMovementDirection = (destination - transform.position);
				manualMovementDirection.y = 0f;
				manualMovementDirection.Normalize();
			}
			else
			{
				manualMovementDirection = Vector3.zero;

				if (Agent.CalculatePath(destination, path))
					Agent.path = path;
			}

			if (BeginMove != null)
				BeginMove();

			return destination;
		}

		public void StopManualMovement()
		{
			if (manualMovementDirection != Vector3.zero)
			{
				Agent.velocity = Vector3.zero;
				Agent.ResetPath();
			}

			manualMovementDirection = Vector3.zero;
		}

		private void Update()
		{
			if (manualMovementDirection == Vector3.zero)
				return;

			float maxTurnDelta = Agent.angularSpeed * 0.5f * Mathf.Deg2Rad * Time.deltaTime;
			float maxSpeedDelta = Agent.acceleration;

			Vector3 desiredVelocity = manualMovementDirection * Agent.speed;
			Vector3 velocity = Vector3.RotateTowards(Agent.velocity, desiredVelocity, maxTurnDelta, maxSpeedDelta);

			Agent.velocity = velocity;
			transform.rotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
		}
	}
}

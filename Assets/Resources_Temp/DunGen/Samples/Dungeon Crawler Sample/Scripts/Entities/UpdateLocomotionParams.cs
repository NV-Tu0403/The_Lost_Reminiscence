using UnityEngine;
using UnityEngine.AI;

namespace DunGen.DungeonCrawler
{
	/// <summary>
	/// Updates movement parameters on the attached animator component
	/// </summary>
	[RequireComponent(typeof(Animator))]
	sealed class UpdateLocomotionParams : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("The agent to pull movment information from")]
		private NavMeshAgent agent = null;

		private Animator animator;
		private int movingPropID;
		private int inputXPropID;
		private int inputZPropID;


		private void Awake()
		{
			animator = GetComponent<Animator>();

			// We cache hashed property IDs instead of using strings every frame
			// for performance and garbage collection purposes
			movingPropID = Animator.StringToHash("Moving");
			inputXPropID = Animator.StringToHash("Input X");
			inputZPropID = Animator.StringToHash("Input Z");
		}

		private void Update()
		{
			Vector3 currentVelocity = agent.velocity;

			animator.SetBool(movingPropID, currentVelocity.sqrMagnitude > 0f);
			animator.SetFloat(inputXPropID, currentVelocity.x);
			animator.SetFloat(inputZPropID, currentVelocity.z);
		}
	}
}

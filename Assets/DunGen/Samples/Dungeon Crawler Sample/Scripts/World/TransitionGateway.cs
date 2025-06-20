using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;

namespace DunGen.DungeonCrawler
{
	sealed class TransitionGateway : MonoBehaviour, IKeyLock
	{
		[SerializeField]
		[Tooltip("The playable director responsible for animating the gate")]
		private PlayableDirector openGateTimeline = null;

		[SerializeField]
		[Tooltip("A NavMesh obstacle that blocks the gate while it's closed")]
		private NavMeshObstacle navMeshObstacle = null;

		[SerializeField]
		[Tooltip("An image that's rendered to the minimap")]
		private GameObject minimapIcon = null;

		private int requiredKeyID;
		private bool isLocked;


		public void OnKeyAssigned(Key key, KeyManager manager)
		{
			requiredKeyID = key.ID;
			isLocked = true;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!isLocked)
				return;

			var objectCollector = other.gameObject.GetComponent<ObjectCollector>();

			if (objectCollector == null)
				return;


			if (objectCollector.HasKey(requiredKeyID))
			{
				navMeshObstacle.enabled = false;
				openGateTimeline.Play();
				objectCollector.RemoveKey(requiredKeyID);

				if (minimapIcon != null)
					minimapIcon.SetActive(false);

				isLocked = false;
			}
			else
			{
				var playerUI = FindObjectOfType<PlayerUI>();
				playerUI.ShowMessage("Missing graveyard key");
			}
		}
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DunGen.DungeonCrawler
{
	sealed class ObjectCollector : MonoBehaviour
	{
		private int gold;
		public int Gold
		{
			get { return gold; }
			set
			{
				gold = value;

				if (GoldChanged != null)
					GoldChanged();
			}
		}

		private List<int> keys = new List<int>();


		[SerializeField]
		private NavMeshAgent agent = null;

		[SerializeField]
		private float pickupRadius = 5f;

		[SerializeField]
		private LayerMask overlapLayerMask = -1;

		public event Action GoldChanged;
		public event Action KeyCollectionChanged;

		private Collider[] overlapBuffer = new Collider[32];


		private void Update()
		{
			if (agent.velocity == Vector3.zero)
				return;

			int overlapCount = Physics.OverlapSphereNonAlloc(transform.position, pickupRadius, overlapBuffer, overlapLayerMask, QueryTriggerInteraction.Collide);

			for (int i = 0; i < overlapCount; i++)
			{
				var collider = overlapBuffer[i];
				var collectible = collider.gameObject.GetComponent<ICollectibleObject>();

				if (collectible != null && collectible.CanPickUp)
					PickUp(collectible);
			}
		}

		private void PickUp(ICollectibleObject collectible)
		{
			collectible.PickUp(this);
			Destroy(((Component)collectible).gameObject);
		}

		public bool HasKey(int keyID)
		{
			return keys.Contains(keyID);
		}

		public void AddKey(int keyID)
		{
			keys.Add(keyID);

			if (KeyCollectionChanged != null)
				KeyCollectionChanged();
		}

		public void RemoveKey(int keyID)
		{
			keys.Remove(keyID);

			if (KeyCollectionChanged != null)
				KeyCollectionChanged();
		}
	}
}

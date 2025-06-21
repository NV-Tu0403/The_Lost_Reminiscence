using UnityEngine;

namespace DunGen.DungeonCrawler
{
	/// <summary>
	/// A simple collectible object that just increments the gold count of the collector
	/// </summary>
	sealed class GoldCollectible : MonoBehaviour, ICollectibleObject
	{
		public bool CanPickUp { get; set; }

		public void PickUp(ObjectCollector collector)
		{
			collector.Gold++;
		}
	}
}

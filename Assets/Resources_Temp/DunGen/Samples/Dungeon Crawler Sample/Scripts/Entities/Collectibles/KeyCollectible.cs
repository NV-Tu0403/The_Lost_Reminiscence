using UnityEngine;

namespace DunGen.DungeonCrawler
{
	sealed class KeyCollectible : MonoBehaviour, ICollectibleObject
	{
		public int KeyID { get; private set; }
		public bool CanPickUp { get; set; }


		public void PickUp(ObjectCollector collector)
		{
			collector.AddKey(KeyID);
		}

		public void SetKeyID(int keyID)
		{
			// We could also change the key's model or colour from here if we wanted to
			KeyID = keyID;
		}
	}
}

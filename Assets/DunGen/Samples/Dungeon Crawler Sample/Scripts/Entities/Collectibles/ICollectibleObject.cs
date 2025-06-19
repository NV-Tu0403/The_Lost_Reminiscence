namespace DunGen.DungeonCrawler
{
	/// <summary>
	/// Interface to implement on any object the player should be able to walk over to pick up.
	/// Pickup logic is implemented in the <see cref="ObjectCollector"/> class
	/// </summary>
	interface ICollectibleObject
	{
		/// <summary>
		/// Can the object be collected?
		/// </summary>
		bool CanPickUp { get; set; }


		/// <summary>
		/// Called when the object is walked over by the player
		/// </summary>
		/// <param name="collector">The player who walked over the object</param>
		void PickUp(ObjectCollector collector);
	}
}

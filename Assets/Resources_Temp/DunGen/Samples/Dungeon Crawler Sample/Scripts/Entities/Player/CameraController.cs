using UnityEngine;

namespace DunGen.DungeonCrawler
{
	/// <summary>
	/// Manages the position & rotation of the overhead player camera
	/// Should be attached to the camera GameObject
	/// </summary>
	sealed class CameraController : MonoBehaviour
	{
		/// <summary>
		/// The transform to look at
		/// </summary>
		public Transform Target = null;

		/// <summary>
		/// How far away the camera should be
		/// </summary>
		public float Distance = 15f;

		/// <summary>
		/// The camera pitch
		/// </summary>
		public float Pitch = 60f;

		/// <summary>
		/// The camera yaw
		/// </summary>
		public float Yaw = 45f;


		/// <summary>
		/// Update the camera transform in LateUpdate() so the player has had a chance to move during Update()
		/// This avoids stuttering
		/// </summary>
		private void LateUpdate()
		{
			if (Target == null)
				return;

			Vector3 targetPosition; Quaternion targetRotation;
			CalculateDestination(out targetPosition, out targetRotation);

			transform.SetPositionAndRotation(targetPosition, targetRotation);
		}

		private void CalculateDestination(out Vector3 position, out Quaternion rotation)
		{
			rotation = Quaternion.Euler(Pitch, Yaw, 0f);
			position = Target.position - rotation * Vector3.forward * Distance;
		}
	}
}

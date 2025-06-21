using UnityEngine;

namespace DunGen
{
	/// <summary>
	/// Controls the position and rotation of the minimap camera
	/// </summary>
	sealed class MinimapCameraController : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("The target transform to follow")]
		private Transform target = null;

		[SerializeField]
		[Tooltip("How height above the target the camera should be")]
		private float height = 20f;

		[SerializeField]
		[Tooltip("Custom rotation offset")]
		private Vector3 rotationOffset = Vector3.zero;


		private void LateUpdate()
		{
			transform.position = target.position + Vector3.up * height;

			Quaternion targetRotation = Quaternion.Euler(90f, 0f, 0f) * Quaternion.Euler(rotationOffset);
			transform.rotation = targetRotation;
		}
	}
}

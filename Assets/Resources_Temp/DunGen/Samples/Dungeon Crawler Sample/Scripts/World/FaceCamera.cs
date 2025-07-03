using UnityEngine;

namespace DunGen.DungeonCrawler
{
	sealed class FaceCamera : MonoBehaviour
	{
		private CameraController playerCamera;


		private void LateUpdate()
		{
			if (playerCamera == null)
				playerCamera = FindObjectOfType<CameraController>();

			if (playerCamera != null)
			{
				Vector3 toCamera = (playerCamera.transform.position - transform.position);
				transform.forward = -toCamera.normalized;
			}
		}
	}
}

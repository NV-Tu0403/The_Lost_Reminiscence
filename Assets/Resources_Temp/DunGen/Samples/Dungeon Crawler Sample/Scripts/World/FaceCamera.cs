using UnityEngine;

namespace DunGen.DungeonCrawler
{
	sealed class FaceCamera : MonoBehaviour
	{
		private CameraController playerCamera;


		private void LateUpdate()
		{
			if (playerCamera == null)
				// ĐÃ SỬA DÒNG NÀY
				playerCamera = FindAnyObjectByType<CameraController>();

			if (playerCamera != null)
			{
				Vector3 toCamera = (playerCamera.transform.position - transform.position);
				transform.forward = -toCamera.normalized;
			}
		}
	}
}
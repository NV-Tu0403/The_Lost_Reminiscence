using UnityEngine;

namespace DunGen.DungeonCrawler
{
	/// <summary>
	/// Continuously spins the GameObject around a local axis
	/// </summary>
	sealed class Spinner : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("The local axis to rotate around")]
		private Vector3 rotationAxis = Vector3.up;

		[SerializeField]
		[Tooltip("How fast to rotate (degrees-per-second)")]
		private float rotationRate = 90f;


		private void Update()
		{
			transform.localRotation *= Quaternion.AngleAxis(rotationRate * Time.deltaTime, rotationAxis);
		}
	}
}

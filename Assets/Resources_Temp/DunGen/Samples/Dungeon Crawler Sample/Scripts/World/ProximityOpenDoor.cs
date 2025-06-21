using System.Collections;
using UnityEngine;

namespace DunGen.DungeonCrawler
{
	/// <summary>
	/// A door that automatically opens as the player approaches it and then closes behind them
	/// </summary>
	sealed class ProximityOpenDoor : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("The transform that is rotated to open/close the door")]
		private Transform hinge = null;

		[SerializeField]
		[Tooltip("How fast (in degrees-per-second) the door can rotate")]
		private float rotationRate = 360f;

		[SerializeField]
		[Tooltip("The local yaw angle (degrees) when open")]
		private float openAngle = 80f;

		[SerializeField]
		[Tooltip("The local yaw angle (degrees) when closed")]
		private float closedAngle = 0f;

		private Door doorComponent;
		private Coroutine runningCoroutine;


		private void Start()
		{
			// A door component should have been automatically aded by DunGen
			doorComponent = GetComponent<Door>();
		}

		public void Open(bool invertAngle)
		{
			doorComponent.IsOpen = true;
			AnimateToAngle((invertAngle) ? -openAngle : openAngle, false);
		}

		public void Close()
		{
			AnimateToAngle(closedAngle, true);
		}

		private void AnimateToAngle(float targetAngle, bool closeDoorWhenComplete)
		{
			if (runningCoroutine != null)
				StopCoroutine(runningCoroutine);

			runningCoroutine = StartCoroutine(AnimateToAngleCoroutine(targetAngle, closeDoorWhenComplete));
		}

		private IEnumerator AnimateToAngleCoroutine(float targetAngle, bool closeDoorWhenComplete)
		{
			float startAngle = hinge.localRotation.eulerAngles.y;
			float angleDistance = Mathf.Abs(Mathf.DeltaAngle(targetAngle, startAngle));
			float duration = angleDistance / rotationRate;

			float time = 0f;
			while (time < duration)
			{
				yield return null;
				time += Time.deltaTime;
				time = Mathf.Min(time, duration);

				float normalizedTime = time / duration;
				float currentAngle = Mathf.LerpAngle(startAngle, targetAngle, normalizedTime);
				hinge.localRotation = Quaternion.Euler(0f, currentAngle, 0f);
			}

			if (closeDoorWhenComplete)
				doorComponent.IsOpen = false;

			runningCoroutine = null;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!other.gameObject.CompareTag("Player"))
				return;

			// Adjust the open angle so the door always opens away from the player
			// --------------------------------------------------------------------
			// Take the vector pointing from the player to the door, and the vector
			// pointing towards the door's world-space forward direction.
			// The dot product of these is positive if the vectors are facing the same
			// direction, and negative if they're facing opposite directions.
			Vector3 toDoor = (transform.position - other.transform.position);
			float dot = Vector3.Dot(toDoor, transform.forward);

			Open(dot < 0f);
		}

		private void OnTriggerExit(Collider other)
		{
			if (!other.gameObject.CompareTag("Player"))
				return;

			Close();
		}
	}
}

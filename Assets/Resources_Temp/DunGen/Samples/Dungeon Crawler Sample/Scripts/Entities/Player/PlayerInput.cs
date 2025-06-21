using UnityEngine;

namespace DunGen.DungeonCrawler
{
	sealed class PlayerInput : MonoBehaviour
	{
		[SerializeField]
		private float clickRepeatInterval = 0.5f;

		[SerializeField]
		private ClickToMove movement = null;
		[SerializeField]
		private ClickableObjectHandler clickableObjectHandler = null;
		[SerializeField]
		private Camera playerCamera = null;

		private float lastClickTime;


		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
				ExitDemo();

			if (Input.GetMouseButton(0))
			{
				bool newlyPressed = Input.GetMouseButtonDown(0);
	
				if(newlyPressed)
					clickableObjectHandler.Click();

				if(	clickableObjectHandler.HoverClickable == null &&
					Time.time >= lastClickTime + clickRepeatInterval)
					MoveToCursor();
			}

			if (Input.GetMouseButtonUp(0))
				movement.StopManualMovement();
		}

		private void MoveToCursor()
		{
			bool newlyPressed = Input.GetMouseButtonDown(0);
			var cursorRay = playerCamera.ScreenPointToRay(Input.mousePosition);
			movement.Click(cursorRay, newlyPressed);

			lastClickTime = Time.time;
		}

		private void ExitDemo()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}
	}
}

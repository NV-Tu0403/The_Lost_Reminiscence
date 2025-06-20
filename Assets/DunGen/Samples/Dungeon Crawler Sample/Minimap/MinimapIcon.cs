using UnityEngine;

namespace DunGen.DungeonCrawler
{
	sealed class MinimapIcon : MonoBehaviour
	{
		[SerializeField]
		private Texture2D icon = null;
		[SerializeField]
		private Renderer iconRenderer = null;
		[SerializeField]
		private Material material = null;

		private Material materialInstance;


		private void Start()
		{
			materialInstance = new Material(material);
			materialInstance.hideFlags = HideFlags.HideAndDontSave;
			materialInstance.mainTexture = icon;

			iconRenderer.material = materialInstance;
			iconRenderer.enabled = true;
		}

		private void OnDestroy()
		{
			if (materialInstance != null)
				Destroy(materialInstance);
		}

		private void Update()
		{
			// Make sure the minimap icons are all facing the same direction
			// This doesn't really shouldn't be done every frame but this is the easiest place to do it
			const float angle = 135f;
			transform.rotation = Quaternion.Euler(0f, angle, 0f);
		}
	}
}

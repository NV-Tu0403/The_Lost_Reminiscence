using UnityEngine;
using UnityEngine.UI;

namespace DunGen.DungeonCrawler
{
	/// <summary>
	/// Renders the minimap to a RawImage on a GameObject named "Minimap"
	/// and the icons to a RawImage on a GameObject named "Minimap Icons"
	/// </summary>
	[RequireComponent(typeof(Camera))]
	sealed class RenderMinimap : MonoBehaviour
	{
		[SerializeField]
		private Material drawMinimapMaterial = null;
		[SerializeField]
		private Camera minimapIconsCamera = null;
		[SerializeField]
		private Material createDistanceFieldMaterial = null;

		private RawImage minimapImage = null;
		private RawImage minimapIconsImage = null;
		private Camera minimapCamera;
		private RenderTexture cameraBuffer;
		private RenderTexture distanceFieldBuffer;
		private RenderTexture outputBuffer;
		private RenderTexture iconsBuffer;
		private Material createDistanceFieldMaterialInstance;


		private void OnEnable()
		{
			minimapCamera = GetComponent<Camera>();

			// Create necessary buffers
			const int inputRes = 512;
			const int outputRes = 512;
			cameraBuffer = new RenderTexture(inputRes, inputRes, 0);
			distanceFieldBuffer = new RenderTexture(inputRes, inputRes, 0);
			outputBuffer = new RenderTexture(outputRes, outputRes, 0);
			iconsBuffer = new RenderTexture(outputRes, outputRes, 0);

			// Setup material
			createDistanceFieldMaterialInstance = new Material(createDistanceFieldMaterial);
			createDistanceFieldMaterialInstance.SetFloat("_TextureSize", inputRes);

			// Tell the minimap camera to render into an off-screen buffer
			minimapCamera.targetTexture = cameraBuffer;

			// Hook the output buffer up to the RawImage component in the UI
			if (minimapImage == null)
			{
				var minimapObject = GameObject.Find("Minimap");
				minimapImage = minimapObject.GetComponent<RawImage>();
			}

			if (minimapIconsImage == null)
			{
				var minimapIconsObject = GameObject.Find("Minimap Icons");
				minimapIconsImage = minimapIconsObject.GetComponent<RawImage>();
			}

			minimapImage.texture = outputBuffer;
			minimapIconsImage.texture = iconsBuffer;

			minimapIconsCamera.targetTexture = iconsBuffer;
		}

		private void OnDisable()
		{
			minimapCamera.targetTexture = null;

			Destroy(cameraBuffer);
			Destroy(distanceFieldBuffer);
			Destroy(outputBuffer);
			Destroy(createDistanceFieldMaterialInstance);

			cameraBuffer = null;
			distanceFieldBuffer = null;
			outputBuffer = null;
			createDistanceFieldMaterialInstance = null;
		}

		private void OnPostRender()
		{
			// After the minimap camera is done rendering, convert the contents to a distance field
			Graphics.Blit(cameraBuffer, distanceFieldBuffer, createDistanceFieldMaterialInstance);

			// Render the distance field as the final minimap
			Graphics.Blit(distanceFieldBuffer, outputBuffer, drawMinimapMaterial);
		}
	}
}

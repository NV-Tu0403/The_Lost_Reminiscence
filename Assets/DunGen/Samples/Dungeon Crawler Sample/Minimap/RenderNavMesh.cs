using UnityEngine;
using UnityEngine.AI;

namespace DunGen.DungeonCrawler
{
	/// <summary>
	/// Creates a renderable mesh from the completed navmesh and renders it every frame
	/// </summary>
	sealed class RenderNavMesh : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("The generator to listen to for an event to let us know when the dungeon generation is complete")]
		private RuntimeDungeon runtimeDungeon = null;

		[SerializeField]
		[Tooltip("Which layer the NavMesh is rendered to")]
		private int renderLayer = 1;

		private Mesh renderableNavMesh;


		private void OnEnable()
		{
			runtimeDungeon.Generator.OnGenerationStatusChanged += OnDungeonGenerationStatusChanged;
		}

		private void OnDisable()
		{
			runtimeDungeon.Generator.OnGenerationStatusChanged -= OnDungeonGenerationStatusChanged;
		}

		private void OnDungeonGenerationStatusChanged(DungeonGenerator generator, GenerationStatus status)
		{
			if (status == GenerationStatus.Complete)
				Refresh();
		}

		public void Refresh()
		{
			if (renderableNavMesh != null)
				Destroy(renderableNavMesh);

			var triangulation = NavMesh.CalculateTriangulation();

			renderableNavMesh = new Mesh();
			renderableNavMesh.vertices = triangulation.vertices;
			renderableNavMesh.SetIndices(triangulation.indices, MeshTopology.Triangles, 0);

			renderableNavMesh.RecalculateBounds();
			renderableNavMesh.RecalculateNormals();
			renderableNavMesh.RecalculateTangents();
		}

		private void Update()
		{
			if(renderableNavMesh != null)
				Graphics.DrawMesh(renderableNavMesh, Matrix4x4.identity, null, renderLayer);
		}
	}
}

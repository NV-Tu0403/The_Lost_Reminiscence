using DunGen.Adapters;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DunGen.DungeonCrawler
{
	/// <summary>
	/// Generates a navigation mesh across the entire dungeon.
	/// DunGen comes with built-in integration for Unity's High Level NavMesh API (see documentation)
	/// but we're doing it manually here for two reasons:
	/// 1. To avoid having to import dependencies for the sample.
	/// 2. We need to use an agent radius that's different from the default value and don't want
	///    to overwrite your project's settings when importing DunGen.
	/// </summary>
	sealed class DungeonCrawlerNavMeshAdapter : NavMeshAdapter
	{
		[SerializeField]
		private LayerMask layerMask = -1;
		[SerializeField]
		private float overrideAgentRadius = 0.3f;

		private NavMeshDataInstance navMeshDataInstance;


		public override void Generate(Dungeon dungeon)
		{
			var root = dungeon.transform;

			navMeshDataInstance.Remove();
			navMeshDataInstance = new NavMeshDataInstance();

			var sources = new List<NavMeshBuildSource>();
			var markups = new List<NavMeshBuildMarkup>();
			NavMeshBuilder.CollectSources(root, layerMask, NavMeshCollectGeometry.RenderMeshes, 0, markups, sources);

			var bounds = CalculateWorldBounds(sources);
			int settingsCount = NavMesh.GetSettingsCount();

			for (int i = 0; i < settingsCount; i++)
			{
				var settings = NavMesh.GetSettingsByID(i);
				settings.agentRadius = overrideAgentRadius;

				var data = NavMeshBuilder.BuildNavMeshData(settings, sources, bounds, root.position, root.rotation);
				navMeshDataInstance = NavMesh.AddNavMeshData(data, root.position, root.rotation);
				navMeshDataInstance.owner = this;
			}
		}

		protected override void OnDisable()
		{
			navMeshDataInstance.Remove();
			base.OnDisable();
		}

		#region NavMesh Helper Methods - Taken from Unity's High-Level NavMesh Components API

		private Bounds CalculateWorldBounds(List<NavMeshBuildSource> sources)
		{
			// Use the unscaled matrix for the NavMeshSurface
			Matrix4x4 worldToLocal = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
			worldToLocal = worldToLocal.inverse;

			var result = new Bounds();
			foreach (var src in sources)
			{
				switch (src.shape)
				{
					case NavMeshBuildSourceShape.Mesh:
						{
							var m = src.sourceObject as Mesh;
							result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, m.bounds));
							break;
						}
					case NavMeshBuildSourceShape.Terrain:
						{
							// Terrain pivot is lower/left corner - shift bounds accordingly
							var t = src.sourceObject as TerrainData;
							result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, new Bounds(0.5f * t.size, t.size)));
							break;
						}
					case NavMeshBuildSourceShape.Box:
					case NavMeshBuildSourceShape.Sphere:
					case NavMeshBuildSourceShape.Capsule:
					case NavMeshBuildSourceShape.ModifierBox:
						result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, new Bounds(Vector3.zero, src.size)));
						break;
				}
			}

			// Inflate the bounds a bit to avoid clipping co-planar sources
			result.Expand(0.1f);
			return result;
		}

		private Bounds GetWorldBounds(Matrix4x4 mat, Bounds bounds)
		{
			var absAxisX = Abs(mat.MultiplyVector(Vector3.right));
			var absAxisY = Abs(mat.MultiplyVector(Vector3.up));
			var absAxisZ = Abs(mat.MultiplyVector(Vector3.forward));
			var worldPosition = mat.MultiplyPoint(bounds.center);
			var worldSize = absAxisX * bounds.size.x + absAxisY * bounds.size.y + absAxisZ * bounds.size.z;
			return new Bounds(worldPosition, worldSize);
		}

		private Vector3 Abs(Vector3 vector)
		{
			return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
		}

		#endregion
	}
}

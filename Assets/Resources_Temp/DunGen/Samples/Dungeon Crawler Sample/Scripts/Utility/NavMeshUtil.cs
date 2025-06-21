using UnityEngine;
using UnityEngine.AI;

namespace DunGen.DungeonCrawler
{
	/// <summary>
	/// A collection fo NavMesh utility methods
	/// </summary>
	static class NavMeshUtil
	{
		public static float CalculatePathLength(NavMeshPath path)
		{
			if (path.corners.Length < 2)
				return 0;

			Vector3 previousCorner = path.corners[0];
			float cumulativeLength = 0.0f;

			for (int i = 0; i < path.corners.Length; i++)
			{
				Vector3 currentCorner = path.corners[i];
				cumulativeLength += Vector3.Distance(previousCorner, currentCorner);
				previousCorner = currentCorner;
			}

			return cumulativeLength;
		}
	}
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DunGen.DungeonCrawler
{
	/// <summary>
	/// An object whose renderer(s) can be disabled when they block line of sight to the player.
	/// This is tested by a raycast in <see cref="ObjectHidingCamera"/>.
	/// These can be nested so that all child objects are hidden when one of them is blocking
	/// line of sight (e.g. the entire wall can become invisible when one segment is affected)
	/// </summary>
	sealed class HideableObject : MonoBehaviour
	{
		#region Statics

		/// <summary>
		/// We cache a map of colliders and their corresponding HideableObjects
		/// since we'll be looking these up a lot during gameplay
		/// </summary>
		private static Dictionary<Collider, HideableObject> hideableObjectsMap = new Dictionary<Collider, HideableObject>();

		/// <summary>
		/// Refresh the cached information about parent-child relationships and the cache
		/// of colliders associated with each HideableObject
		/// </summary>
		public static void RefreshHierarchies()
		{
			// Cleat existing
			foreach (var obj in hideableObjectsMap.Values)
			{
				obj.SetVisible(true);
				obj.parent = null;
				obj.children.Clear();
			}

			hideableObjectsMap.Clear();

			// Re-populate map of colliders
			foreach (var obj in FindObjectsOfType<HideableObject>())
				if(obj.Collider != null)
					hideableObjectsMap[obj.Collider] = obj;

			// Re-calculate parent-child relationships between objects
			foreach (var obj in hideableObjectsMap.Values)
			{
				obj.parent = obj.FindParent(obj.transform);

				if (obj.parent != null)
					obj.parent.children.Add(obj);
			}
		}

		/// <summary>
		/// Find the root object for the given collider
		/// </summary>
		/// <param name="collider">Collider to check</param>
		/// <returns>The root HideableObject, or null if none was found</returns>
		public static HideableObject GetRootHideableByCollider(Collider collider)
		{
			HideableObject obj;

			if (hideableObjectsMap.TryGetValue(collider, out obj))
				return GetRoot(obj);
			else
				return null;
		}

		/// <summary>
		/// Recursively check up the object hierarchy to find the top-most HideableObject
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <returns>The root object in the HideableObject hierarchy</returns>
		private static HideableObject GetRoot(HideableObject obj)
		{
			if (obj.parent == null)
				return obj;
			else
				return GetRoot(obj.parent);
		}

		#endregion

		[Tooltip("All renderers that should be hidden when this object is occluding. Can be empty if this object is just used to group other HideableObjects")]
		public List<Renderer> Renderers = new List<Renderer>();
		[Tooltip("The collider that raycasts are tested against to determine if this object is occluding. Can be empty if this object is just used to group other HideableObjects")]
		public Collider Collider = null;

		// Cached hierarchy information
		private HideableObject parent;
		private List<HideableObject> children = new List<HideableObject>();


		/// <summary>
		/// Recursively find the top-most HideableObject in a GameObject hierarchy.
		/// </summary>
		/// <param name="transform">The first transform to check</param>
		/// <returns>The root HideableObject, or null if none is found</returns>
		private HideableObject FindParent(Transform transform)
		{
			var parent = transform.parent;

			if (parent != null)
			{
				var hideableObject = parent.GetComponent<HideableObject>();

				if (hideableObject != null)
					return hideableObject;
				else
					return FindParent(parent);
			}
			else
				return null;
		}

		/// <summary>
		/// Sets the visibility of this object and all of its children.
		/// </summary>
		/// <param name="visible">Is visible?</param>
		public void SetVisible(bool visible)
		{
			if (Renderers != null)
			{
				// Instead of setting the renderer's "visible" property to hide the objects,
				// we set the shadow casting mode to "Shadows Only" so the objects are invisible, but we still get shadows
				foreach(var renderer in Renderers)
					renderer.shadowCastingMode = visible ? ShadowCastingMode.On : ShadowCastingMode.ShadowsOnly;
			}

			foreach (var child in children)
				child.SetVisible(visible);
		}
	}
}

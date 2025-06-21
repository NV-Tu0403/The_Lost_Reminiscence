using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DunGen.DungeonCrawler
{
	/// <summary>
	/// Performs some game-specific logic after the dungeon has been generated.
	/// Must be attached to the same GameObject as the dungeon generator
	/// </summary>
	[RequireComponent(typeof(RuntimeDungeon))]
	sealed class DungeonSetup : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("The player prefab to spawn once the dungeon is complete")]
		private GameObject playerPrefab = null;

		private PlayerUI playerUI;
		private RuntimeDungeon runtimeDungeon;
		private GameObject spawnedPlayerInstance;


		private void OnEnable()
		{
			playerUI = FindObjectOfType<PlayerUI>();

			runtimeDungeon = GetComponent<RuntimeDungeon>();
			runtimeDungeon.Generator.OnGenerationStatusChanged += OnDungeonGenerationStatusChanged;
		}

		private void OnDisable()
		{
			runtimeDungeon.Generator.OnGenerationStatusChanged -= OnDungeonGenerationStatusChanged;
		}

		private void OnDungeonGenerationStatusChanged(DungeonGenerator generator, GenerationStatus status)
		{
			// We're only interested in completion events
			if (status != GenerationStatus.Complete)
				return;

			// If there's already a player instance, destroy it. We'll spawn a new one
			if (spawnedPlayerInstance != null)
				Destroy(spawnedPlayerInstance);

			// Find an object inside the start tile that's marked with the PlayerSpawn component
			var playerSpawn = generator.CurrentDungeon.MainPathTiles[0].GetComponentInChildren<PlayerSpawn>();

			Vector3 spawnPosition = playerSpawn.transform.position;
			spawnedPlayerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
			playerUI.SetPlayer(spawnedPlayerInstance);

			// All hideable objects are spawned by now,
			// we can cache some information for later use
			HideableObject.RefreshHierarchies();
		}
	}
}

using UnityEngine;
using DunGen;
using DunGen.DungeonCrawler;
using System.Collections;

namespace Loc_Backend
{
    public class TestTriggerZone : MonoBehaviour
    {
        public DungeonTrigger dungeonTrigger; // Assign in Inspector
        private RuntimeDungeon runtimeDungeon;
        private GameObject playerToTeleport;

        private void Start()
        {
            runtimeDungeon = dungeonTrigger.dungeonGenerator;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerToTeleport = other.gameObject;
                dungeonTrigger.GenerateDungeon();
                StartCoroutine(WaitForGenerationAndTeleport());
            }
        }

        private IEnumerator WaitForGenerationAndTeleport()
        {
            var generator = runtimeDungeon.Generator;
            while (generator.IsGenerating)
                yield return null;

            if (playerToTeleport != null)
            {
                var playerSpawn = generator.CurrentDungeon.MainPathTiles[0].GetComponentInChildren<PlayerSpawn>();
                if (playerSpawn != null)
                {
                    playerToTeleport.transform.position = playerSpawn.transform.position;
                }
                playerToTeleport = null;
            }
        }
    }
}
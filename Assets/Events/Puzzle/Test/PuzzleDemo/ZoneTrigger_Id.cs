using System.Collections.Generic;
using UnityEngine;

namespace Events.Puzzle.Test.PuzzleDemo
{
    public class ZoneTriggerId : MonoBehaviour
    { 
        [SerializeField] private Puzzle3 puzzle3;

        private void Awake()
        {
            if (puzzle3 == null)
                puzzle3 = FindObjectOfType<Puzzle3>();
        }

        private void OnTriggerEnter(Collider other)
        {
            var testController = other.GetComponent<TestController>();
            if (testController != null && puzzle3 != null)
            {
                foreach (var id in puzzle3.ghosts)
                {
                    id.SetChaseTarget(testController);
                }
            }
        }
    }
}

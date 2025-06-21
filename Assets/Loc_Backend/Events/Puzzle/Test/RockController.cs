using System;
using UnityEngine;

namespace Events.Puzzle.Test
{
    public class RockController : MonoBehaviour
    {
        private Rigidbody rb;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
            }
            else
            {
                Debug.LogWarning("Rigidbody component is missing on the RockController GameObject.");
            }
        }
        
        public void TestDrop()
        {
            if (rb != null)
            {
                rb.useGravity = true;
            }
            else
            {
                Debug.LogWarning("Rigidbody component is missing on the RockController GameObject.");
            }
        }
    }
}

using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tu_Develop.Import.Scripts
{
    public class FaSetupBlackBoard : MonoBehaviour
    {
        /// <summary>
        /// Thiết lập các biến trên Blackboard cho Fa system
        /// </summary>

        [Header("Blackboard Variables")] [SerializeField]
        private BlackboardReference playerConfigBb;
        private void Awake()
        {
            SetupBlackboardVariables();
        }

        private void SetupBlackboardVariables()
        {
            if (playerConfigBb != null)
            {
                // setting idleConfig in faAgent
                playerConfigBb.SetVariableValue("PlayerHealth", 1);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                playerConfigBb.GetVariableValue(("PlayerHealth"), out int health);
                health++;
                playerConfigBb.SetVariableValue("PlayerHealth", health);
            }
        }
    }
}

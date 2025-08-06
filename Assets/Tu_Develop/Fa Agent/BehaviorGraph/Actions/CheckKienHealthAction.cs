using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CheckKienHealth", story: "Return current [Playerhealth] health value from [Player]", category: "Action", id: "dcaa22efda636f1ec732ac8a417cc314")]
public partial class CheckKienHealthAction : Action
{
    [SerializeReference] public BlackboardVariable<int> Playerhealth;
    [SerializeReference] public BlackboardVariable<GameObject> Player;

    protected override Status OnStart()
    {
        // if (Player == null || Playerhealth == null)
        // {
        //     Debug.LogError("Player or Playerhealth is not set.");
        //     return Status.Failure;
        // }
        //
        // var playerHealthComponent = Player.Value.GetComponent<PlayerHealth>();
        // if (playerHealthComponent == null)
        // {
        //     Debug.LogError("Player does not have a PlayerHealth component.");
        //     return Status.Failure;
        // }
        //
        // Playerhealth.Value = playerHealthComponent.gameObject;
        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}


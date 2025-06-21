using System;
using Unity.Behavior;
using UnityEngine;
using Modifier = Unity.Behavior.Modifier;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FollowMainCharacter", story: "If [Distance] between [Self] and [Kien]", category: "Flow", id: "381f8dcb63f8bc14a5664400501b2ee4")]
public partial class FollowMainCharacterModifier : Modifier
{
    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}


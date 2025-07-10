using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Check Distance", story: "Check [Distance] Between [Self] and [Target]", category: "Action", id: "f07487a8b84ae1c14518aed97aea054d")]
public partial class CheckDistanceAction : Action
{
    [SerializeReference] public BlackboardVariable<float> Distance;
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Self.Value != null && Target.Value != null)
        {
            Distance.Value = Vector3.Distance(Self.Value.transform.position, Target.Value.transform.position);
        }
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}


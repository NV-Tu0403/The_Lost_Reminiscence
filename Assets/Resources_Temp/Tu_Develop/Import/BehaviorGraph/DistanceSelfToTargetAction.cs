using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "DistanceSelfToTarget", story: "Check distance between [Self] and [Target] and save it to [DistanceToTarget]", category: "Action", id: "eb62a0d1cf407ad1f4409a5007d23b44")]
public partial class DistanceSelfToTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> DistanceToTarget;
    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        var selfGO = Self.Value;
        var targetGO = Target.Value;

        if (selfGO == null)
        {
            Debug.LogWarning("Self is not set.");
            return Status.Failure;
        }
        if (targetGO == null)
        {
            Debug.LogWarning("Target is not set.");
            return Status.Failure;
        }

        var distance = Vector3.Distance(selfGO.transform.position, targetGO.transform.position);
        DistanceToTarget.Value = distance;
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}


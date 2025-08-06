using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CheckDistance", story: "Return [Distance] between [Self] and [Target]", category: "Action", id: "4e0ce73556a6fc315bd82aa248b7182d")]
public partial class CheckDistanceAction : Action
{
    [SerializeReference] public BlackboardVariable<float> Distance;
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Transform> Target;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Self?.Value == null || Target?.Value == null)
        {
            Distance.Value = -1f; // hoặc giá trị báo lỗi
            return Status.Failure;
        }
        float distance = Vector3.Distance(Self.Value.transform.position, Target.Value.position);
        Distance.Value = distance;
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}


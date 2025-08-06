using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Get Random Position", story: "Return [Position] In [Radius] Around [Self]", category: "Action", id: "737949d13f60cee4adca0e2c615e5249")]
public partial class GetRandomTransformAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> Position;
    [SerializeReference] public BlackboardVariable<float> Radius;
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    protected override Status OnStart()
    {
        // 1. Lấy giá trị thực từ các biến trên Blackboard
        var selfGo = Self.Value;
        var radiusValue = Radius.Value;
        
        // 2. Kiểm tra xem các giá trị đầu vào có hợp lệ không
        if (selfGo == null)
        {
            Debug.LogError("GetRandomTransformAction: 'Self' chưa được gán trên Blackboard!");
            return Status.Failure; // Báo thất bại để Behavior Tree biết
        }
        
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radiusValue;
        randomDirection += selfGo.transform.position;
        
        // 4. Tìm điểm hợp lệ gần nhất trên NavMesh
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randomDirection, out navHit, radiusValue, NavMesh.AllAreas))
        {
            Position.Value = navHit.position;
            return Status.Success;
        }
        else
        {
            // Nếu không tìm thấy điểm hợp lệ nào trên NavMesh, báo thất bại
            return Status.Failure;
        }
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}


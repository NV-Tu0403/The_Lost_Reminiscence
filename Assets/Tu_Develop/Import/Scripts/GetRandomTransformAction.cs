using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Get Random POS", story: "Return [POS] In [Radius] Around [Self]", category: "Action", id: "737949d13f60cee4adca0e2c615e5249")]
public partial class GetRandomTransformAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> POS;
    [SerializeReference] public BlackboardVariable<float> Radius;
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    protected override Status OnStart()
    {
        // 1. Lấy giá trị thực từ các biến trên Blackboard
        var selfGo = Self.Value;
        var radiusValue = Radius.Value;
        var posTransform = POS.Value;
        
        // 2. Kiểm tra xem các giá trị đầu vào có hợp lệ không
        if (selfGo == null)
        {
            Debug.LogError("GetRandomTransformAction: 'Self' chưa được gán trên Blackboard!");
            return Status.Failure; // Báo thất bại để Behavior Tree biết
        }
        
        if (posTransform == null)
        {
            Debug.LogError("GetRandomTransformAction: Input 'POS' is null. This usually means the 'RandomPosition' variable on the Blackboard hasn't been assigned a helper Transform from FaAgent.cs.");
            return Status.Failure; 
        }
        
        
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radiusValue;
        randomDirection += selfGo.transform.position;
        
        // 4. Tìm điểm hợp lệ gần nhất trên NavMesh
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randomDirection, out navHit, radiusValue, NavMesh.AllAreas))
        {
            // Nếu tìm thấy một điểm hợp lệ...
            // 5. Cập nhật vị trí của Transform 'POS'
            POS.Value.position = navHit.position;
            
            // 6. Báo hiệu hành động đã hoàn thành thành công
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


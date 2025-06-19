using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SelectRandomPoint", story: "Return [RandomPoint] in [Range] of [Self]", category: "Action", id: "ea7ee6b55c38b6b8e5aa1c4e122a5a21")]
public partial class SelectRandomPointAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> RandomPoint;
    [SerializeReference] public BlackboardVariable<float> Range;
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    private Vector3 m_ChosenPosition;
    private bool m_PositionFound;

    protected override Status OnStart()
    {
        m_PositionFound = false;

        var origin = Self.Value.transform.position;
        const int maxTries = 10;

        for (int i = 0; i < maxTries; i++)
        {
            // 1. Sinh vector ngẫu nhiên trong sphere
            Vector3 randomDir = UnityEngine.Random.insideUnitSphere * Range.Value;
            Vector3 target = origin + randomDir;

            // 2. Sample lên NavMesh
            if (NavMesh.SamplePosition(target, out NavMeshHit hit, Range.Value, NavMesh.AllAreas))
            {
                m_ChosenPosition = hit.position;
                m_PositionFound = true;
                break;
            }
        }

        // Nếu không tìm được, fallback: tự đứng yên
        if (!m_PositionFound)
            m_ChosenPosition = origin;

        // Ghi lên Blackboard
        RandomPoint.Value = m_ChosenPosition;


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


using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "RayCastPuzzle", story: "Return [PuzzleList] in [Range] of [Self]", category: "Conditions", id: "9fa896cd8b52d6689cab802c93044be1")]
public partial class RayCastPuzzleCondition : Condition
{
    [SerializeReference] public BlackboardVariable<List<GameObject>> PuzzleList;
    [SerializeReference] public BlackboardVariable<float> Range;
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private bool m_FoundPuzzle = false;
    private LayerMask m_PuzzleLayerMask;


    public override bool IsTrue()
    {
        return true;
    }

    public override void OnStart()
    {
        PuzzleList.Value = new List<GameObject>();
        m_FoundPuzzle = false;
        // Raycast Circle with Range 
        if (Self.Value == null)
        {
            Debug.LogWarning("Self is not set.");
            return;
        }
        Vector3 origin = Self.Value.transform.position;
        float radius = Range.Value;

        m_PuzzleLayerMask = LayerMask.GetMask("Puzzle");

        Collider[] hitColliders = Physics.OverlapSphere(origin, radius, m_PuzzleLayerMask);
        List<GameObject> puzzlesInRange = new List<GameObject>();

        foreach (var hit in hitColliders)
        {
            if (hit.gameObject.layer == LayerMask.NameToLayer("Puzzle"))
            {
                puzzlesInRange.Add(hit.gameObject);
                m_FoundPuzzle = true;
            }
        }

        PuzzleList.Value = puzzlesInRange;
    }

    public override void OnEnd()
    {
    }
}

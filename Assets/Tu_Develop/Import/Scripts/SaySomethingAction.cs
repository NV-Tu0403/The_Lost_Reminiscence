using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using TMPro; // Required for Text Mesh Pro

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SaySomething", story: "[Self] Say [Something]", category: "Action", id: "b157faca6ba084999a38cb2e2364e10a")]
public partial class SaySomethingAction : Action
{
    [SerializeReference] public BlackboardVariable<TextMeshPro> Self;
    [SerializeReference] public BlackboardVariable<List<string>> Something;

    protected override Status OnStart()
    {
        // Ensure Self and Something are valid
        if (Self.Value == null || Something.Value == null || Something.Value.Count == 0)
        {
            Debug.LogWarning("Self or Something is not properly initialized.");
            return Status.Failure;
        }

        var textMeshPro = Self.Value;

        // Randomly select a string from Something
        var randomIndex = UnityEngine.Random.Range(0, Something.Value.Count);
        var randomString = Something.Value[randomIndex];

        // Assign the selected string to the text property
        textMeshPro.text = randomString;

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


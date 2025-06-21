using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ReturnStringFormTalkList", story: "Return [String] form Talk's [List]", category: "Action", id: "c89b1771cc7278987fa1d57bf4a39ae8")]
public partial class ReturnStringFormTalkListAction : Action
{
    [SerializeReference] public BlackboardVariable<string> String;
    [SerializeReference] public BlackboardVariable<List<string>> List;

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


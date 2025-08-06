using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "CheckObjectState", story: "[Object] State Is [Null]", category: "Conditions", id: "33af6c4ce226e369b2ed47cad6bc1148")]
public partial class CheckObjectStateCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Object;
    [SerializeReference] public BlackboardVariable<bool> Null;

    public override bool IsTrue()
    {
        bool isNull = Object?.Value == null;
        Null.Value = isNull;
        return isNull;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}

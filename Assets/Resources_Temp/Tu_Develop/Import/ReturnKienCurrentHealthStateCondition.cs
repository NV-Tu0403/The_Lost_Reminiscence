using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Return Kien Current Health State", story: "Health of [Kien] is [below] than [Number]", category: "Variable Conditions", id: "55732baa1a46abefe9fbce8fec73f4a8")]
public partial class ReturnKienCurrentHealthStateCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Kien;
    [Comparison(comparisonType: ComparisonType.All)]
    [SerializeReference] public BlackboardVariable<ConditionOperator> Below;
    [SerializeReference] public BlackboardVariable<int> Number;

    private bool result = false;

    public override bool IsTrue()
    {
        return result;
    }

    public override void OnStart()
    {
        //result = false;
        //if (Kien.Value == null)
        //{
        //    Debug.LogWarning("Script or Kien is not set.");
        //    return;
        //}
        //if (Kien.Value.TryGetComponent<MainCharacter>(out MainCharacter kienScript))
        //{
        //    if (kienScript.CurrentHealth < Number.Value)
        //    {
        //        result = true;
        //    }
        //}
        //else
        //{
        //    Debug.LogWarning("Kien does not have a MainCharacter component.");
        //}
    }

    public override void OnEnd()
    {
    }
}

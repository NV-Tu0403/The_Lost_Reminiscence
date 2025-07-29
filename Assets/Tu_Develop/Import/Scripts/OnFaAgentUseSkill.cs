using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/OnFaAgentUseSkill")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "OnFaAgentUseSkill", message: "Agent has use [Skill]", category: "Events",
    id: "1a973804959195de50e47891debe089e")]
public sealed partial class OnFaAgentUseSkill : EventChannel<string>
{
    
}


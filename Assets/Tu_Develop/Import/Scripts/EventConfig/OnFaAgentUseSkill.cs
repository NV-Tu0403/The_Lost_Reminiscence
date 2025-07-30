using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

#if UNITY_EDITOR
namespace Tu_Develop.Import.Scripts.EventConfig
{
    [CreateAssetMenu(menuName = "Behavior/Event Channels/OnFaAgentUseSkill")]
#endif
    [Serializable, GeneratePropertyBag]
    [EventChannelDescription(name: "OnFaAgentUseSkill", message: "Agent has use [Skill]", category: "Events", id: "1a973804959195de50e47891debe089e")]
    public sealed partial class OnFaAgentUseSkill : EventChannel<string> { }
}


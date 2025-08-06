using System;
using System.Globalization;
using UnityEngine.UI;
using TMPro;
using Tu_Develop.Import.Scripts;
using Tu_Develop.Import.Scripts.EventConfig;
using UnityEngine;

namespace Code.UI.Gameplay
{
    [Serializable]
    public class UIFaSkillSetting
    {
        public Image state;
        public TextMeshProUGUI cooldownText;
    }
    
    
    public class UIFaSkill : MonoBehaviour
    {
        [Header("Event Channels")] [SerializeField]
        private OnFaAgentUseSkill useSkillEventChannel;
        [SerializeField] private FaAgentEventChannel faAgentEventChannel;


        [Header("Skills Components")] 
        [SerializeField] private UIFaSkillSetting skill1;
        [SerializeField] private UIFaSkillSetting skill2;
        [SerializeField] private UIFaSkillSetting skill3;
        
        private void OnEnable()
        {
            if (faAgentEventChannel != null)
            {
                faAgentEventChannel.OnFaAgentReady += SetFaAgentReference;
                useSkillEventChannel.Event += OnUseSkill;
            }
        }
        
        private void OnDisable()
        {
            if (faAgentEventChannel != null)
            {
                faAgentEventChannel.OnFaAgentReady -= SetFaAgentReference;
                useSkillEventChannel.Event -= OnUseSkill;
            }
        }

        private void SetFaAgentReference(FaAgent agent)
        {
            // set skill 1
            skill1.state.GetComponent<Image>().color = Color.green;
            skill1.cooldownText.gameObject.SetActive(false);
            
            // set skill 2
            skill2.state.GetComponent<Image>().color = Color.green;
            skill2.cooldownText.gameObject.SetActive(false);
            
            // set skill 3
            skill3.state.GetComponent<Image>().color = Color.green;
            skill3.cooldownText.gameObject.SetActive(false);
        }
        
        private void OnUseSkill(string value0)
        {
            switch (value0)
            {
                case "GuideSignal":
                    skill1.state.GetComponent<Image>().color = Color.red;
                    skill1.cooldownText.gameObject.SetActive(true);
                    break;
                case "KnowledgeLight":
                    skill2.state.GetComponent<Image>().color = Color.red;
                    skill2.cooldownText.gameObject.SetActive(true);
                    break;
                case "ProtectiveAura":
                    skill3.state.GetComponent<Image>().color = Color.red;
                    skill3.cooldownText.gameObject.SetActive(true);
                    break;
            }
        }

        public void UpdateCoolDown(string key, float value)
        {
            var formattedValue = value.ToString(CultureInfo.InvariantCulture);
            if (value <= 0)
            {
                UpdateState(key, true);
                return;
            }
            else
            {
                UpdateState(key, false);
            }
            switch (key)
            {
                case "GuideSignal":
                    skill1.cooldownText.text = formattedValue;
                    break;
                case "KnowledgeLight":
                    skill2.cooldownText.text = formattedValue;
                    break;
                case "ProtectiveAura": 
                    skill3.cooldownText.text = formattedValue;
                    break;
                default:
                    Debug.LogWarning($"Unknown skill key: {key}");
                    break;
            }
        }


        private void UpdateState(string key, bool isReady)
        {
            var color = isReady ? Color.green : Color.red;
            switch (key)
            {
                case "GuideSignal":
                    skill1.state.GetComponent<Image>().color = color;
                    break;
                case "KnowledgeLight":
                    skill2.state.GetComponent<Image>().color = color;
                    break;
                case "ProtectiveAura":
                    skill3.state.GetComponent<Image>().color = color;
                    break;
                default:
                    Debug.LogWarning($"Unknown skill key: {key}");
                    break;
            }
            
            if (isReady)
            {
                switch (key)
                {
                    case "GuideSignal":
                        skill1.cooldownText.gameObject.SetActive(false);
                        break;
                    case "KnowledgeLight":
                        skill2.cooldownText.gameObject.SetActive(false);
                        break;
                    case "ProtectiveAura":
                        skill3.cooldownText.gameObject.SetActive(false);
                        break;
                }
            }
            
        }
    }
}

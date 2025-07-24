using UnityEngine;
using Code.Boss;

namespace Code.Boss.Testing
{
    /// <summary>
    /// Giả lập 3 kỹ năng của Fa và tích hợp với Boss system
    /// </summary>
    public class FaSkillSimulator : MonoBehaviour
    {
        [Header("Fa Skills Configuration")]
        [SerializeField] private float radarCooldown = 10f;
        [SerializeField] private float secondSkillCooldown = 8f;
        [SerializeField] private float thirdSkillCooldown = 15f;
        
        [Header("Skill Effects")]
        [SerializeField] private GameObject radarEffectPrefab;
        [SerializeField] private GameObject secondSkillEffectPrefab;
        [SerializeField] private GameObject thirdSkillEffectPrefab;
        
        // Cooldown tracking
        private float lastRadarUse;
        private float lastSecondSkillUse;
        private float lastThirdSkillUse;
        
        // Skill availability
        public bool CanUseRadar => Time.time >= lastRadarUse + radarCooldown;
        public bool CanUseSecondSkill => Time.time >= lastSecondSkillUse + secondSkillCooldown;
        public bool CanUseThirdSkill => Time.time >= lastThirdSkillUse + thirdSkillCooldown;

        private void Start()
        {
            // Register for boss integration events
            RegisterForBossEvents();
        }

        private void RegisterForBossEvents()
        {
            FaBossIntegration.OnRequestFaSkill += HandleSkillRequest;
            FaBossIntegration.OnSoulCountChanged += HandleSoulCountChanged;
            FaBossIntegration.OnBossVulnerable += HandleBossVulnerability;
        }

        private void HandleSkillRequest(string skillName)
        {
            Debug.Log($"[Fa Simulator] Boss system requested skill: {skillName}");
            
            switch (skillName.ToLower())
            {
                case "radar":
                    if (CanUseRadar)
                    {
                        UseRadarSkill();
                    }
                    else
                    {
                        Debug.Log("[Fa Simulator] Radar skill is on cooldown!");
                    }
                    break;
                    
                default:
                    Debug.Log($"[Fa Simulator] Unknown skill requested: {skillName}");
                    break;
            }
        }

        private void HandleSoulCountChanged(int soulCount)
        {
            Debug.Log($"[Fa Simulator] Soul count changed to: {soulCount}");
            
            if (soulCount >= 2 && CanUseRadar)
            {
                Debug.Log("[Fa Simulator] Suggesting to use Radar skill to clear souls!");
                ShowRadarSuggestion();
            }
        }

        private void HandleBossVulnerability(bool isVulnerable)
        {
            Debug.Log($"[Fa Simulator] Boss vulnerability changed: {isVulnerable}");
            
            if (isVulnerable)
            {
                Debug.Log("[Fa Simulator] Boss is vulnerable! Good time to attack!");
                ShowAttackOpportunity();
            }
        }

        #region Fa Skills Implementation

        /// <summary>
        /// Kỹ năng 1: Radar - Phá hủy tất cả Soul
        /// </summary>
        public void UseRadarSkill()
        {
            if (!CanUseRadar)
            {
                Debug.Log("[Fa Simulator] Radar skill is on cooldown!");
                return;
            }
            
            lastRadarUse = Time.time;
            Debug.Log("[Fa Simulator] === RADAR SKILL ACTIVATED ===");
            
            // Create radar effect
            CreateRadarEffect();
            
            // Destroy all souls through boss integration
            FaBossIntegration.NotifyFaSkillUsed("Radar", true);
            
            Debug.Log("[Fa Simulator] All souls destroyed by Radar!");
        }

        /// <summary>
        /// Kỹ năng 2: Shield/Protection - Bảo vệ player khỏi damage
        /// </summary>
        public void UseSecondSkill()
        {
            if (!CanUseSecondSkill)
            {
                Debug.Log("[Fa Simulator] Second skill is on cooldown!");
                return;
            }
            
            lastSecondSkillUse = Time.time;
            Debug.Log("[Fa Simulator] === PROTECTION SKILL ACTIVATED ===");
            
            // Create protection effect
            CreateSecondSkillEffect();
            
            // Apply protection buff (example)
            StartCoroutine(ApplyProtectionBuff());
            
            FaBossIntegration.NotifyFaSkillUsed("Protection", true);
        }

        /// <summary>
        /// Kỹ năng 3: Reveal - Hiện thị bóng thật trong Decoy state
        /// </summary>
        public void UseThirdSkill()
        {
            if (!CanUseThirdSkill)
            {
                Debug.Log("[Fa Simulator] Third skill is on cooldown!");
                return;
            }
            
            lastThirdSkillUse = Time.time;
            Debug.Log("[Fa Simulator] === REVEAL SKILL ACTIVATED ===");
            
            // Create reveal effect
            CreateThirdSkillEffect();
            
            // Reveal real decoy
            RevealRealDecoy();
            
            FaBossIntegration.NotifyFaSkillUsed("Reveal", true);
        }

        #endregion

        #region Skill Effects

        private void CreateRadarEffect()
        {
            // Create expanding radar wave effect
            GameObject radarEffect = new GameObject("RadarEffect");
            radarEffect.transform.position = transform.position;
            
            // Add visual components
            var renderer = radarEffect.AddComponent<MeshRenderer>();
            var meshFilter = radarEffect.AddComponent<MeshFilter>();
            
            // Create simple radar visualization
            meshFilter.mesh = CreateRadarMesh();
            
            // Cleanup after effect
            Destroy(radarEffect, 3f);
            
            Debug.Log("[Fa Simulator] Radar visual effect created");
        }

        private void CreateSecondSkillEffect()
        {
            // Create protection shield around player
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                GameObject shieldEffect = new GameObject("ProtectionShield");
                shieldEffect.transform.position = player.transform.position;
                shieldEffect.transform.parent = player.transform;
                
                // Add shield visual
                var renderer = shieldEffect.AddComponent<MeshRenderer>();
                var meshFilter = shieldEffect.AddComponent<MeshFilter>();
                meshFilter.mesh = CreateShieldMesh();
                
                // Make shield semi-transparent
                var material = new Material(Shader.Find("Standard"));
                material.color = new Color(0, 1, 1, 0.3f);
                material.SetFloat("_Mode", 3); // Transparent mode
                renderer.material = material;
                
                Destroy(shieldEffect, 5f);
            }
        }

        private void CreateThirdSkillEffect()
        {
            // Create reveal effect that highlights real decoy
            var decoys = FindObjectsOfType<DecoyBehavior>();
            
            foreach (var decoy in decoys)
            {
                if (decoy.IsReal)
                {
                    // Highlight real decoy
                    GameObject highlight = new GameObject("RevealHighlight");
                    highlight.transform.position = decoy.transform.position + Vector3.up * 2;
                    highlight.transform.parent = decoy.transform;
                    
                    // Add glowing effect
                    var light = highlight.AddComponent<Light>();
                    light.color = Color.green;
                    light.intensity = 2f;
                    light.range = 5f;
                    
                    Destroy(highlight, 3f);
                    
                    Debug.Log("[Fa Simulator] Real decoy revealed!");
                    break;
                }
            }
        }

        #endregion

        #region Helper Methods

        private Mesh CreateRadarMesh()
        {
            // Simple circle mesh for radar effect
            Mesh mesh = new Mesh();
            int segments = 32;
            Vector3[] vertices = new Vector3[segments + 1];
            int[] triangles = new int[segments * 3];
            
            vertices[0] = Vector3.zero;
            
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2;
                vertices[i + 1] = new Vector3(Mathf.Cos(angle) * 10, 0.1f, Mathf.Sin(angle) * 10);
                
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = (i + 1) % segments + 1;
            }
            
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            
            return mesh;
        }

        private Mesh CreateShieldMesh()
        {
            // Simple sphere mesh for shield
            return Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
        }

        private System.Collections.IEnumerator ApplyProtectionBuff()
        {
            Debug.Log("[Fa Simulator] Protection buff active for 5 seconds");
            // Here you would implement actual protection logic
            yield return new WaitForSeconds(5f);
            Debug.Log("[Fa Simulator] Protection buff expired");
        }

        private void RevealRealDecoy()
        {
            var decoys = FindObjectsOfType<DecoyBehavior>();
            foreach (var decoy in decoys)
            {
                if (decoy.IsReal)
                {
                    Debug.Log("[Fa Simulator] Real decoy location revealed!");
                    // Additional reveal logic here
                    break;
                }
            }
        }

        private void ShowRadarSuggestion()
        {
            // UI suggestion to use radar skill
            Debug.Log("[Fa Simulator] UI: Radar skill suggested!");
        }

        private void ShowAttackOpportunity()
        {
            // UI indication of attack opportunity
            Debug.Log("[Fa Simulator] UI: Attack opportunity!");
        }

        #endregion

        #region Public Methods for Manual Testing

        [System.Obsolete("Use for testing only")]
        public void TestRadarSkill()
        {
            UseRadarSkill();
        }

        [System.Obsolete("Use for testing only")]
        public void TestSecondSkill()
        {
            UseSecondSkill();
        }

        [System.Obsolete("Use for testing only")]
        public void TestThirdSkill()
        {
            UseThirdSkill();
        }

        #endregion

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(320, 10, 300, 150));
            GUILayout.Label("=== FA SKILLS DEBUG ===");
            
            // Radar skill
            string radarStatus = CanUseRadar ? "READY" : $"CD: {(radarCooldown - (Time.time - lastRadarUse)):F1}s";
            GUILayout.Label($"Q - Radar: {radarStatus}");
            
            // Second skill
            string secondStatus = CanUseSecondSkill ? "READY" : $"CD: {(secondSkillCooldown - (Time.time - lastSecondSkillUse)):F1}s";
            GUILayout.Label($"E - Protection: {secondStatus}");
            
            // Third skill
            string thirdStatus = CanUseThirdSkill ? "READY" : $"CD: {(thirdSkillCooldown - (Time.time - lastThirdSkillUse)):F1}s";
            GUILayout.Label($"R - Reveal: {thirdStatus}");
            
            GUILayout.Space(10);
            GUILayout.Label($"Active Souls: {FaBossIntegration.GetCurrentSoulCount()}");
            GUILayout.Label($"Boss Phase: {FaBossIntegration.GetCurrentBossPhase()}");
            
            GUILayout.EndArea();
        }

        private void OnDestroy()
        {
            // Cleanup event subscriptions
            FaBossIntegration.OnRequestFaSkill -= HandleSkillRequest;
            FaBossIntegration.OnSoulCountChanged -= HandleSoulCountChanged;
            FaBossIntegration.OnBossVulnerable -= HandleBossVulnerability;
        }
    }
}

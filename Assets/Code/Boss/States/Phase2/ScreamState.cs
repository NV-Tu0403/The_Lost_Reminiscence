using System.Collections;
using Code.Boss.States.Shared;
using UnityEngine;

namespace Code.Boss.States.Phase2
{
    /// <summary>
    /// Phase 2 - Scream State: Phát âm thanh chê trách và hiệu ứng
    /// </summary>
    public class ScreamState : BossState
    {
        private float castTimer;
        private float skillTimer;
        private bool isCasting = true;
        private bool skillActivated = false;
        private bool playerHitBoss = false;

        public override void Enter()
        {
            castTimer = 0f;
            skillTimer = 0f;
            isCasting = true;
            skillActivated = false;
            playerHitBoss = false;
            
            Debug.Log("[Boss State] Entered ScreamState - Casting Scream skill");
            BossEventSystem.Trigger(BossEventType.ScreamStarted);
            BossEventSystem.Trigger(BossEventType.SkillCasted, new BossEventData { stringValue = "Scream" });
        }

        public override void Update()
        {
            if (isCasting) HandleCasting();
            else HandleSkillActive();
        }

        private void HandleCasting()
        {
            castTimer += Time.deltaTime;
            
            // Update skill cast progress for UI
            var progress = castTimer / Config.phase2.screamCastTime;
            BossEventSystem.Trigger(BossEventType.SkillCastProgress, new BossEventData(progress));
            if (castTimer >= Config.phase2.screamCastTime) ActivateSkill();
        }

        private void ActivateSkill()
        {
            isCasting = false;
            skillActivated = true;
            
            BossEventSystem.Trigger(BossEventType.SkillInterrupted); // Ẩn UI cast bar
            Debug.Log("[Boss State] ScreamState - Scream activated with effects");
            
            // Apply scream effects
            ApplyScreamEffects();
            
            // Play scream sound
            if (Config.audioConfig.screamSound != null)
            {
                BossController.PlaySound(Config.audioConfig.screamSound, Config.audioConfig.sfxVolume);
            }
        }

        private void ApplyScreamEffects()
        {
            ApplyScreenShake();     
            ApplyVisionShrink();  
        }

        private void ApplyScreenShake()
        {
            // Demo: Shake main camera for 0.5s
            var cam = Camera.main;
            if (cam != null)
            {
                BossController.StartCoroutine(ScreenShakeCoroutine(cam, Config.phase2.screenShakeIntensity, 3f));
            }
        }

        private IEnumerator ScreenShakeCoroutine(Camera cam, float intensity, float duration)
        {
            var originalPos = cam.transform.localPosition;
            var elapsed = 0f;
            var x = Random.Range(-1f, 1f) * intensity;
            var y = Random.Range(-1f, 1f) * intensity;
            while (elapsed < duration)
            {
                cam.transform.localPosition = originalPos + new Vector3(x, y, 0);
                elapsed += Time.deltaTime;
                yield return null;
            }
            cam.transform.localPosition = originalPos;
        }

        private void ApplyVisionShrink()
        {
            BossController.StartCoroutine(VisionShrinkOverlayCoroutine(1f));
        }

        private IEnumerator VisionShrinkOverlayCoroutine(float duration)
        {
            // Create overlay canvas and image
            var cam = Camera.main;
            if (cam == null) yield break;
            var canvasGO = new GameObject("VisionShrinkCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            var imgGO = new GameObject("VisionShrinkImage");
            imgGO.transform.SetParent(canvasGO.transform);
            var img = imgGO.AddComponent<UnityEngine.UI.Image>();
            img.color = Color.black;
            img.rectTransform.anchorMin = Vector2.zero;
            img.rectTransform.anchorMax = Vector2.one;
            img.rectTransform.offsetMin = Vector2.zero;
            img.rectTransform.offsetMax = Vector2.zero;
            img.material = null;
            // Animate alpha mask inward
            float elapsed = 0f;
            while (elapsed < duration * 0.5f)
            {
                float t = elapsed / (duration * 0.5f);
                img.color = new Color(0, 0, 0, Mathf.Lerp(0f, 1f, t));
                elapsed += Time.deltaTime;
                yield return null;
            }
            img.color = new Color(0, 0, 0, 1f);
            // Hold for a moment
            yield return new WaitForSeconds(duration * 0.2f);
            // Animate alpha mask outward
            elapsed = 0f;
            while (elapsed < duration * 0.3f)
            {
                float t = elapsed / (duration * 0.3f);
                img.color = new Color(0, 0, 0, Mathf.Lerp(1f, 0f, t));
                elapsed += Time.deltaTime;
                yield return null;
            }
            img.color = new Color(0, 0, 0, 0f);
            Object.Destroy(canvasGO);
        }

        private void HandleSkillActive()
        {
            skillTimer += Time.deltaTime;
            
            if (skillTimer >= Config.phase2.screamDuration)
            {
                EndScreamState();
            }
        }

        private void EndScreamState()
        {
            if (playerHitBoss)
            {
                BossController.TakeDamage(1);
                BossController.ChangeState(new AngryState());
            }
            else
            {
                BossEventSystem.Trigger(BossEventType.PlayerTakeDamage, new BossEventData(1));
                BossController.ChangeState(new SoulState());
            }
        }

        public override void Exit()
        {
            BossEventSystem.Trigger(BossEventType.SkillCasted, 
                new BossEventData { stringValue = "RemoveEffects" });
        }

        public override void OnTakeDamage()
        {
            if (isCasting && CanBeInterrupted())
            {
                BossEventSystem.Trigger(BossEventType.SkillInterrupted);
                BossController.ChangeState(new AngryState());
            }
            else if (skillActivated)
            {
                playerHitBoss = true;
            }
        }

        public override bool CanBeInterrupted() => true;
        public override bool CanTakeDamage() => isCasting;
    }
}

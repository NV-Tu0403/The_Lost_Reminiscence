using UnityEngine;
using Code.Boss.States.Shared;

namespace Code.Boss
{
    /// <summary>
    /// Hành vi của Memory Fragment - mảnh ghép kí ức
    /// </summary>
    public class MemoryFragmentBehavior : MonoBehaviour
    {
        private bool collected = false;
        
        private void Start()
        {
            // Add visual effects for memory fragment
            AddGlowEffect();
        }

        private void AddGlowEffect()
        {
            // Add rotating and glowing effect
            var renderer = GetComponent<Renderer>();
            if (renderer == null)
            {
                // Create a simple cube as memory fragment
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.parent = transform;
                cube.transform.localPosition = Vector3.zero;
                renderer = cube.GetComponent<Renderer>();
            }
            
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Standard"));
                material.color = Color.cyan;
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.cyan * 0.5f);
                renderer.material = material;
            }
        }

        private void Update()
        {
            if (!collected)
            {
                // Rotate the fragment
                transform.Rotate(0, 90 * Time.deltaTime, 0);
                
                // Add floating effect
                float hover = Mathf.Sin(Time.time * 2f) * 0.2f;
                Vector3 pos = transform.position;
                pos.y += hover * Time.deltaTime;
                transform.position = pos;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !collected)
            {
                CollectFragment();
            }
        }

        private void CollectFragment()
        {
            collected = true;
            
            // Trigger collection event
            BossEventSystem.Trigger(BossEventType.SkillCasted, 
                new BossEventData { stringValue = "MemoryFragmentCollected", gameObject = gameObject });
            
            // Play collection effect and sound
            // Add particle effects here
            
            // Destroy after collection
            Destroy(gameObject, 0.5f);
        }
    }
}

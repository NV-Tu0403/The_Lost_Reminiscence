using System;
using System.Collections;
using UnityEngine;

namespace _MyGame.Codes.Puzzle.OpenGate
{
    public class NoteGlowController : MonoBehaviour
    {
        private static readonly int emissionColor = Shader.PropertyToID("_EmissionColor");
        
        [SerializeField] public Renderer[] noteRenderers;
        [SerializeField] private Color glowColor = new Color(6f, 3f, 0f);
        [SerializeField] private float glowIntensity = 1f;
        [SerializeField] public float delayBetweenNotes = 0.2f;

        public Action OnGlowComplete;
        // Invoked each time a note starts glowing (passes the note index)
        public Action<int> OnNoteGlow;

        private Material[] noteMats;

        private void Awake()
        {
            noteMats = new Material[noteRenderers.Length];
            for (var i = 0; i < noteRenderers.Length; i++)
            {
                noteMats[i] = noteRenderers[i].material;
                noteMats[i].SetColor(emissionColor, Color.black);
                noteMats[i].DisableKeyword("_EMISSION");

                noteMats[i].globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                DynamicGI.SetEmissive(noteRenderers[i], Color.black);
            }
        }

        public void TriggerGlowSequence()
        {
            StartCoroutine(GlowNotesSequentially());
        }

        private IEnumerator GlowNotesSequentially()
        {
            for (var i = 0; i < noteMats.Length; i++)
            {
                noteMats[i].EnableKeyword("_EMISSION");
                noteMats[i].SetColor(emissionColor, glowColor * glowIntensity);
                // Notify per-note glow
                OnNoteGlow?.Invoke(i);
                yield return new WaitForSeconds(delayBetweenNotes);
            }

            OnGlowComplete?.Invoke();
        }

        public void ForceGlowAll()
        {
            StopAllCoroutines();
            for (var i = 0; i < noteMats.Length; i++)
            {
                noteMats[i].EnableKeyword("_EMISSION");
                noteMats[i].SetColor(emissionColor, glowColor * glowIntensity);
                // Notify per-note glow in forced mode as well
                OnNoteGlow?.Invoke(i);
            }
            OnGlowComplete?.Invoke();
        }
    }
}
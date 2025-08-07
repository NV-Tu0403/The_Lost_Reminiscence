using System;
using System.Collections;
using UnityEngine;

namespace Code.Puzzle.OpenGate
{
    public class NoteGlowController : MonoBehaviour
    {
        [SerializeField] public Renderer[] noteRenderers;
        [SerializeField] private Color glowColor = new Color(6f, 3f, 0f);
        [SerializeField] private float glowIntensity = 1f;
        [SerializeField] public float delayBetweenNotes = 0.2f;

        public Action OnGlowComplete;

        private Material[] _noteMats;

        private void Awake()
        {
            _noteMats = new Material[noteRenderers.Length];
            for (int i = 0; i < noteRenderers.Length; i++)
            {
                _noteMats[i] = noteRenderers[i].material;
                _noteMats[i].SetColor("_EmissionColor", Color.black);
                _noteMats[i].DisableKeyword("_EMISSION");

                _noteMats[i].globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                DynamicGI.SetEmissive(noteRenderers[i], Color.black);
            }
        }

        public void TriggerGlowSequence()
        {
            StartCoroutine(GlowNotesSequentially());
        }

        private IEnumerator GlowNotesSequentially()
        {
            for (int i = 0; i < _noteMats.Length; i++)
            {
                _noteMats[i].EnableKeyword("_EMISSION");
                _noteMats[i].SetColor("_EmissionColor", glowColor * glowIntensity);
                yield return new WaitForSeconds(delayBetweenNotes);
            }

            OnGlowComplete?.Invoke();
        }

        public void ForceGlowAll()
        {
            StopAllCoroutines();
            for (int i = 0; i < _noteMats.Length; i++)
            {
                _noteMats[i].EnableKeyword("_EMISSION");
                _noteMats[i].SetColor("_EmissionColor", glowColor * glowIntensity);
            }
            OnGlowComplete?.Invoke();
        }
    }
}
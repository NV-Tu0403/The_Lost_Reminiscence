using System;
using System.Collections.Generic;
using Loc_Backend.Scripts;
using UnityEngine;

// Script này quản lý các sự kiện trong game,
// bao gồm cả việc khởi chạy các sự kiện như cắt cảnh, thay đổi bản đồ, đối thoại, v.v.

namespace Script.GameEventSystem
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }
        private Dictionary<string, Action> eventMap;

        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            else Instance = this;
            InitializeEventDictionary();
        }

        
        // Phương thức này sẽ được gọi để khởi chạy một sự kiện dựa trên ID của nó.
        public void PlayEvent(string eventId)
        {
            if (eventMap.TryGetValue(eventId, out var action))
                action.Invoke();
            else if (eventId.StartsWith("Dialogue_"))
                DialogueManager.Instance.StartDialogue(eventId);
            
            // Mo rong sau.
            // Vi du:
            // CutsceneManager.Instance.Play(eventId);
            // PuzzleManager.Instance.Load(eventId);
            else
                Debug.LogWarning($"[EventManager] Unknown event: {eventId}");
        }
        
        public void OnEventFinished()
        {
            Debug.Log("[EventManager] Event finished.");
            // TODO: Add logic to handle when an event is finished (e.g. trigger next event, update state, etc.)
        }

        private void InitializeEventDictionary()
        {
            eventMap = new Dictionary<string, Action>
            {
                // { "Cutscene_Something", () => CutsceneManager.Instance.Play("Something") },
            };
        }
    }
}


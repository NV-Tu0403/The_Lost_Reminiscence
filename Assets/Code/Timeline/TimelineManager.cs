using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using System;
using Code.GameEventSystem;

namespace Code.Timeline
{
    /// <summary>
    /// TimelineManager is responsible for managing timeline events in the game.
    /// </summary>
    public class TimelineManager : MonoBehaviour
    {
        [Header("UI Elements")]
        public GameObject timelinePanel;
        public Button skipButton;
        public PlayableDirector playableDirector;

        private Action onFinished;

        private void Awake()
        {
            timelinePanel.SetActive(false);
            skipButton.onClick.AddListener(SkipTimeline);
        }

        private void OnEnable()
        {
            EventBus.Subscribe("StartTimeline", OnStartTimelineEvent);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe("StartTimeline", OnStartTimelineEvent);
        }

        private void OnStartTimelineEvent(object data)
        {
            if (data is not BaseEventData eventData) return;
            StartTimeline(eventData.eventId, eventData.OnFinish);
        }

        private void StartTimeline(string timelineId, Action onFinished)
        {
            this.onFinished = onFinished;
            timelinePanel.SetActive(true);
            // Load timeline asset from Resources/Timeline folder
            var timelineAsset = Resources.Load<PlayableAsset>($"Timelines/{timelineId}");
            if (timelineAsset == null)
            {
                Debug.LogError($"Timeline asset not found: {timelineId}");
                timelinePanel.SetActive(false);
                onFinished?.Invoke();
                return;
            }
            playableDirector.playableAsset = timelineAsset;
            playableDirector.stopped += OnTimelineFinished;
            playableDirector.Play();
        }

        private void SkipTimeline()
        {
            playableDirector.Stop();
        }

        private void OnTimelineFinished(PlayableDirector director)
        {
            playableDirector.stopped -= OnTimelineFinished;
            timelinePanel.SetActive(false);
            onFinished?.Invoke();
        }
    }
}
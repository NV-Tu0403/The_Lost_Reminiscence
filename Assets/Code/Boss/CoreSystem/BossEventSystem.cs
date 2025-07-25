using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Boss
{
    /// <summary>
    /// Hệ thống sự kiện riêng cho Boss, tách biệt với GameEventSystem
    /// </summary>
    public static class BossEventSystem
    {
        private static Dictionary<BossEventType, List<Action<BossEventData>>> eventListeners = 
            new Dictionary<BossEventType, List<Action<BossEventData>>>();

        /// <summary>
        /// Đăng ký lắng nghe một sự kiện boss
        /// </summary>
        public static void Subscribe(BossEventType eventType, Action<BossEventData> listener)
        {
            if (!eventListeners.ContainsKey(eventType))
            {
                eventListeners[eventType] = new List<Action<BossEventData>>();
            }
            
            eventListeners[eventType].Add(listener);
        }

        /// <summary>
        /// Hủy đăng ký lắng nghe sự kiện
        /// </summary>
        public static void Unsubscribe(BossEventType eventType, Action<BossEventData> listener)
        {
            if (eventListeners.ContainsKey(eventType))
            {
                eventListeners[eventType].Remove(listener);
            }
        }

        /// <summary>
        /// Phát sự kiện boss
        /// </summary>
        public static void Trigger(BossEventType eventType, BossEventData data = null)
        {
            if (eventListeners.ContainsKey(eventType))
            {
                foreach (var listener in eventListeners[eventType])
                {
                    try
                    {
                        listener?.Invoke(data);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error in BossEvent listener for {eventType}: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Xóa tất cả listeners
        /// </summary>
        public static void ClearAllListeners()
        {
            eventListeners.Clear();
        }
    }

    /// <summary>
    /// Các loại sự kiện boss
    /// </summary>
    public enum BossEventType
    {
        // Phase Events
        PhaseChanged,
        BossSpawned,
        BossDefeated,
        
        // State Events
        StateChanged,
        SkillCasted,
        SkillInterrupted,
        
        // Combat Events
        BossTakeDamage,
        PlayerTakeDamage,
        DecoyHit,
        RealDecoyHit,
        FakeDecoyHit,
        
        // Soul Events
        SoulSpawned,
        SoulDestroyed,
        
        // Skill-specific Events
        LureStarted,
        MockStarted,
        DecoyStarted,
        SoulStateStarted,
        FearZoneCreated,
        ScreamStarted,
        
        // UI Events
        HealthChanged,
        SkillCastProgress,
        
        // Fa Integration Events
        RequestRadarSkill, // Yêu cầu Fa sử dụng Radar skill
        RequestOtherSkill, // Yêu cầu Fa sử dụng skill khác
        FaSkillUsed        // Fa đã sử dụng skill
    }

    /// <summary>
    /// Dữ liệu kèm theo sự kiện boss
    /// </summary>
    [Serializable]
    public class BossEventData
    {
        public int intValue;
        public float floatValue;
        public string stringValue;
        public Vector3 position;
        public GameObject gameObject;
        public object customData;

        public BossEventData() { }

        public BossEventData(int value)
        {
            intValue = value;
        }

        public BossEventData(float value)
        {
            floatValue = value;
        }

        public BossEventData(string value)
        {
            stringValue = value;
        }

        public BossEventData(Vector3 pos)
        {
            position = pos;
        }

        public BossEventData(GameObject obj)
        {
            gameObject = obj;
        }
    }
}

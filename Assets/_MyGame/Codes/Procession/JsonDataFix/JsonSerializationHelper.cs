using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using _MyGame.Codes.Procession;
using Script.Procession;
using Script.Procession.Conditions;
using Script.Procession.Reward.Base;
using UnityEngine;

/// <summary>
/// Sử dụng Newtonsoft.Json để serialize/deserialize GameProgression.
/// Tạo ConditionConverter và RewardConverter để ánh xạ trường Type (CollectItem, DefeatEnemy, Item, Experience) sang các lớp con cụ thể.
/// TypeNameHandling.None đảm bảo JSON không chứa thông tin lớp Unity-specific, tạo ra cấu trúc sạch.
/// </summary>
public static class JsonSerializationHelper
{
    public static string SerializeGameProgression(GameProgression progression)
    {
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None, // Không lưu thông tin lớp
            Formatting = Formatting.Indented,
            Converters = new List<JsonConverter> { new ConditionConverter(), new RewardConverter() }
        };
        return JsonConvert.SerializeObject(progression, settings);
    }

    public static GameProgression DeserializeGameProgression(string json)
    {
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            Converters = new List<JsonConverter> { new ConditionConverter(), new RewardConverter() }
        };
        return JsonConvert.DeserializeObject<GameProgression>(json, settings);
    }

    private class ConditionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Condition);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            string type = jo["Type"]?.Value<string>();

            switch (type)
            {
                case "CollectItem":
                    return jo.ToObject<InteractCondition>(serializer);
                case "DefeatEnemy":
                    return jo.ToObject<DefeatEnemyCondition>(serializer);
                default:
                    throw new JsonSerializationException($"Unknown Condition Type: {type}");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

    private class RewardConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Reward);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            string type = jo["Type"]?.Value<string>();

            switch (type)
            {
                case "Item":
                    return jo.ToObject<ItemReward>(serializer);
                case "Experience":
                    return jo.ToObject<ExperienceReward>(serializer);
                default:
                    throw new JsonSerializationException($"Unknown Reward Type: {type}");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
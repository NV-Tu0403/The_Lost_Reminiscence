using System.Collections.Generic;
using UnityEngine;

public class FaAgent : MonoBehaviour, FaInterface
{
    private Dictionary<string, float> cooldownTimers = new Dictionary<string, float>();

    public float GetCooldownRemaining(string skillName)
    {
        return cooldownTimers.ContainsKey(skillName) ? cooldownTimers[skillName] : 0;
    }

    public bool IsSkillAvailable(string skillName)
    {
        return !cooldownTimers.ContainsKey(skillName) || cooldownTimers[skillName] <= 0;
    }

    public void OnPlayerCommand(string command)
    {
        // Giao tiếp từ phía player → Fa phản hồi
        Debug.Log($"Fa nhận lệnh: {command}");
    }

    public void UpdateLearning(float deltaTime)
    {
        // Cập nhật cơ chế tự học
    }

    public void UseEgoLight()
    {
        if (!IsSkillAvailable("EgoLight")) return;
        // Làm chậm kẻ địch bóng tối
        cooldownTimers["EgoLight"] = 25f;
    }

    public void UseGuideSignal(Vector3 targetPosition)
    {
        if (!IsSkillAvailable("GuideSignal")) return;
        // Gọi animation + hiệu ứng ánh sáng
        cooldownTimers["GuideSignal"] = 10f;
    }

    public void UseKnowledgeLight(Vector3 areaCenter)
    {
        if (!IsSkillAvailable("KnowledgeLight")) return;
        // Chiếu sáng khu vực
        cooldownTimers["KnowledgeLight"] = 15f;
    }

    public void UseLightRadar()
    {
        if (!IsSkillAvailable("LightRadar")) return;
        // Gọi hiện vật thể ẩn
        cooldownTimers["LightRadar"] = 12f;
    }

    public void UseProtectiveAura(GameObject target)
    {
        if (!IsSkillAvailable("ProtectiveAura")) return;
        // Tạo lá chắn quanh player
        cooldownTimers["ProtectiveAura"] = 20f;
    }

    void Update()
    {
        // Giảm cooldown mỗi frame
        List<string> keys = new List<string>(cooldownTimers.Keys);
        foreach (string key in keys)
        {
            cooldownTimers[key] = Mathf.Max(0, cooldownTimers[key] - Time.deltaTime);
        }
    }
}

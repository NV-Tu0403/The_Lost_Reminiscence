using UnityEngine;

public interface FaInterface
{
    void UseGuideSignal(Vector3 targetPosition);
    void UseKnowledgeLight(Vector3 areaCenter);
    void UseProtectiveAura(GameObject target);
    void UseLightRadar();
    void UseEgoLight();

    void OnPlayerCommand(string command);
    void UpdateLearning(float deltaTime);

    bool IsSkillAvailable(string skillName);
    float GetCooldownRemaining(string skillName);
}

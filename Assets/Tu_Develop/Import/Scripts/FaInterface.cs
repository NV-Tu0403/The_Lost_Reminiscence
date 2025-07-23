using UnityEngine;

public interface FaInterface
{
    void UseGuideSignal(); // Tín hiệu dẫn lối                  
    void UseKnowledgeLight(); // Ánh sáng tri thức
    void UseProtectiveAura(bool self); // Vần sáng bảo hộ (true = Fa, false = Player)
    void OnPlayerCommand(string command);
    //void UpdateLearning(float deltaTime);
    bool IsSkillAvailable(string skillName);
}

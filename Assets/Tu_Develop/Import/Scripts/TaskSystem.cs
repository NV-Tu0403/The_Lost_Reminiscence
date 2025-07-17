using UnityEngine;

public enum TaskType
{
    MoveTo,
    UseSkill
    // Có thể bổ sung thêm các loại task khác sau này
}

public class FaTask
{
    public TaskType Type;
    public Vector3? TargetPosition; // Dùng cho MoveTo hoặc vị trí dùng skill
    public string? SkillName; // Có thể null nếu không phải UseSkill
    // Có thể thêm các trường khác nếu cần

    public FaTask(TaskType type)
    {
        Type = type;
    }
} 
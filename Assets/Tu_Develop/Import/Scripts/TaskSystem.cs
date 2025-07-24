#nullable enable
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
    public Transform? TaskPosition;
    public string? SkillName;
    public bool? TargetObject;
    public FaTask(TaskType type)
    {
        Type = type;
    }
} 
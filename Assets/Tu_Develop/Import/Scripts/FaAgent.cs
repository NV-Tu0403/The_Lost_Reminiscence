#nullable enable
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

public class FaAgent : MonoBehaviour, FaInterface
{
    private Transform? _targetPositionHelper;
    private Dictionary<string, float> cooldownTimers = new Dictionary<string, float>();

    // Bổ sung TaskQueue quản lý task cho Fa
    public TaskQueue taskQueue = new TaskQueue();

    // Hàm thêm task vào queue
    public void AddTask(FaTask task)
    {
        taskQueue.AddTask(task);
    }

    // Hàm lấy task tiếp theo
    public FaTask? GetNextTask()
    {
        return taskQueue.GetNextTask();
    }

    // Kiểm tra còn task không
    public bool HasTask()
    {
        return taskQueue.HasTask();
    }

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

        var parts = command.Trim().Split(' ');
        if (parts.Length == 0) return;

        if (parts[0].ToLower() == "move" && parts.Length == 4)
        {
            // Lệnh: move x y z
            if (float.TryParse(parts[1], out float x) && float.TryParse(parts[2], out float y) && float.TryParse(parts[3], out float z))
            {
                if (_targetPositionHelper != null)
                {
                    _targetPositionHelper.position = new Vector3(x, y, z);
                    var task = new FaTask(TaskType.MoveTo)
                    {
                        TaskPosition = _targetPositionHelper
                    };
                    AddTask(task);
                    Debug.Log($"Đã thêm task MoveTo: {_targetPositionHelper.position}");
                }
            }
        }
        else if (parts[0].ToLower() == "useskill" && parts.Length >= 2)
        {
            // Lệnh: useskill skillName [x y z]
            var task = new FaTask(TaskType.UseSkill)
            {
                SkillName = parts[1]
            };
            if (parts.Length == 5 && float.TryParse(parts[2], out float x) && float.TryParse(parts[3], out float y) && float.TryParse(parts[4], out float z))
            {
                if (_targetPositionHelper != null)
                {
                    _targetPositionHelper.position = new Vector3(x, y, z);
                    task.TaskPosition = _targetPositionHelper;
                }
            }
            AddTask(task);
            Debug.Log($"Đã thêm task UseSkill: {task.SkillName} tại {task.TaskPosition?.position}");
        }
        else
        {
            Debug.LogWarning("Lệnh không hợp lệ. Ví dụ: 'move 1 2 3' hoặc 'useskill EgoLight' hoặc 'useskill GuideSignal 1 2 3'");
        }
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

    public BehaviorGraphAgent faBHA;

    private void Start()
    {
        faBHA = GetComponent<BehaviorGraphAgent>();
        var go = new GameObject("FaTargetPositionHelper");
        _targetPositionHelper = go.transform;
    }

    void Update()
    {
        // Giảm cooldown mỗi frame (giữ nguyên code cũ)
        List<string> keys = new List<string>(cooldownTimers.Keys);
        foreach (string key in keys)
        {
            cooldownTimers[key] = Mathf.Max(0, cooldownTimers[key] - Time.deltaTime);
        }

        // Xử lý task: khi idle và có task thì đẩy dữ liệu vào blackboard
        if (HasTask())
        {
            var task = GetNextTask();
            if (task != null)
            {
                var playerTaskType = PlayerTaskType.None;
                if (task.Type == TaskType.MoveTo)
                {
                    playerTaskType = PlayerTaskType.Move;
                }
                // Thêm các điều kiện else if cho các loại skill khác nếu cần
                // else if (task.Type == TaskType.UseSkill) { ... }

                faBHA.BlackboardReference.SetVariableValue("TaskFromPlayer", playerTaskType);

                // Nếu TargetPosition là thì cần kiểm tra null
                if (task.TaskPosition != null)
                    faBHA.BlackboardReference.SetVariableValue("TaskPosition", task.TaskPosition.position);
                else
                    faBHA.BlackboardReference.SetVariableValue("TaskPosition", Vector3.zero);
                faBHA.BlackboardReference.SetVariableValue("SkillName", task.SkillName ?? "");
                faBHA.BlackboardReference.SetVariableValue("TaskType", task.Type.ToString());
                faBHA.BlackboardReference.SetVariableValue("PlayerControl", true);
            }
        }
    }

    public bool ActivePlayerControl(bool v)
    {
        try
        {
            if (v)
            {
                faBHA.BlackboardReference.SetVariableValue("PlayerControl", true);
            }
            else
            {
                faBHA.BlackboardReference.SetVariableValue("PlayerControl", false);
            }
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Lỗi khi thay đổi chế độ chỉ huy NPC: {ex.Message}");
            return false;
        }
    }

}

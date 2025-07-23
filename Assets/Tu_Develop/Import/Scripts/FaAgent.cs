#nullable enable
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

public class FaAgent : MonoBehaviour, FaInterface
{
    [Header("References")] public GameObject? playerObject;
    private Transform? _targetPositionHelper;
    private readonly Dictionary<string, float> _cooldownTimers = new Dictionary<string, float>();
    // Bổ sung TaskQueue quản lý task cho Fa
    private readonly TaskQueue _taskQueue = new TaskQueue();
    // Hàm thêm task vào queue
    public void AddTask(FaTask task)
    {
        _taskQueue.AddTask(task);
    }
    // Hàm lấy task tiếp theo
    public FaTask? GetNextTask()
    {
        return _taskQueue.GetNextTask();
    }
    // Kiểm tra còn task không
    public bool HasTask()
    {
        return _taskQueue.HasTask();
    }
    public bool IsSkillAvailable(string skillName)
    {
        return !_cooldownTimers.ContainsKey(skillName) || _cooldownTimers[skillName] <= 0;
    }
    
    // Hàm này sẽ được gọi từ Behavior Graph để bắt đầu đếm ngược
    public void StartSkillCooldown(string skillName, float duration)
    {
        if (_cooldownTimers.ContainsKey(skillName))
        {
            _cooldownTimers[skillName] = duration;
        }
        else
        {
            _cooldownTimers.Add(skillName, duration);
        }
        Debug.Log($"[FaAgent] Skill '{skillName}' bắt đầu cooldown {duration} giây.");
    }

    public void OnPlayerCommand(string command)
    {
        Debug.Log($"Fa nhận lệnh: {command}");
        var parts = command.Trim().Split(' ');
        if (parts.Length == 0) return;
        // MOVE
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
        // SKILL
        else if (parts[0].ToLower() == "useskill" && parts.Length >= 2)
        {
            var skillName = parts[1]; // Skill name/number
            
            // Check cooldown
            if (!IsSkillAvailable(skillName))
            {
                Debug.Log($"[FaAgent] Skill '{skillName}' chưa hồi chiêu.");
                return;
            }

            var task = new FaTask(TaskType.UseSkill)
            {
                SkillName = skillName
            };

            if (parts.Length > 2)
            {
                var targetKeyword = parts[2].ToLower();
                if (targetKeyword == "player")
                {
                    task.TargetObject = playerObject;
                }
                else
                {
                    task.TargetObject = this.gameObject;
                }
                
                AddTask(task);
                Debug.Log($"Đã thêm task UseSkill: {task.SkillName} trên mục tiêu: {task.TargetObject?.name}");
            }
        }
        else
        {
            Debug.LogWarning("Lệnh không hợp lệ.");
        }
    }
    public BehaviorGraphAgent? faBha;

    private void Start()
    {
        faBha = GetComponent<BehaviorGraphAgent>();
        var go = new GameObject("FaTargetPositionHelper");
        _targetPositionHelper = go.transform;

        if (playerObject == null) playerObject = GameObject.Find("Player");
    }

    void Update()
    {
        // Giảm cooldown mỗi frame
        List<string> keys = new List<string>(_cooldownTimers.Keys);
        foreach (string key in keys)
        {
            _cooldownTimers[key] = Mathf.Max(0, _cooldownTimers[key] - Time.deltaTime);
        }

        if (faBha == null) return;
        
        // 2. Đồng bộ hóa trạng thái cooldown LÊN Blackboard
        // Tên skill trong C# phải khớp với tên trong hàm IsSkillAvailable
        // Tên biến trên Blackboard phải khớp với những gì bạn đã tạo
        faBha.BlackboardReference.SetVariableValue("IsVangSangBaoHoAvailable", IsSkillAvailable("EgoLight"));
        faBha.BlackboardReference.SetVariableValue("IsTinHieuDanLoiAvailable", IsSkillAvailable("GuideSignal"));
        faBha.BlackboardReference.SetVariableValue("IsAnhSangTriThucAvailable", IsSkillAvailable("KnowledgeLight"));
        
        // 3. Xử lý giao task (như đã sửa ở lần trước)
        bool isGraphBusy;
        faBha.BlackboardReference.GetVariableValue("IsTaskRunning", out isGraphBusy);
        
        // Chỉ xử lý task mới khi Graph rảnh và hàng đợi có task
        if (!isGraphBusy && HasTask())
        {
            var task = GetNextTask();
            if (task != null)
            {
                var playerTaskType = PlayerTaskType.None;

                if (task.Type == TaskType.MoveTo)
                {
                    playerTaskType = PlayerTaskType.Move;
                }
                // *** PHẦN LOGIC BỊ THIẾU MÀ BẠN CẦN THÊM VÀO ***
                else if (task.Type == TaskType.UseSkill)
                {
                    // Dựa vào SkillName (hiện là "1", "2", "3" từ input) để chọn đúng enum
                    switch (task.SkillName)
                    {
                        case "1":
                            playerTaskType = PlayerTaskType.TinHieuDanLoi;
                            break;
                        case "2":
                            playerTaskType = PlayerTaskType.AnhSangTriThuc;
                            break;
                        case "3":
                            playerTaskType = PlayerTaskType.VangSangBaoHo;
                            break;
                        default:
                            Debug.LogWarning($"Skill name '{task.SkillName}' không hợp lệ!");
                            playerTaskType = PlayerTaskType.None;
                            break;
                    }
                }

                // Nếu không có task nào hợp lệ thì không làm gì cả
                if (playerTaskType == PlayerTaskType.None) return;

                // Đẩy dữ liệu vào Blackboard
                faBha.BlackboardReference.SetVariableValue("TaskFromPlayer", playerTaskType);
                
                if (task.TaskPosition != null)
                    faBha.BlackboardReference.SetVariableValue("TaskPosition", task.TaskPosition);
                if (task.TargetObject != null)
                    faBha.BlackboardReference.SetVariableValue("TargetObject", task.TargetObject);
                // Bật chế độ PlayerControl để Graph nhận task
                faBha.BlackboardReference.SetVariableValue("PlayerControl", true);
            }
        }
    }

    public bool ActivePlayerControl(bool v)
    {
        try
        {
            faBha?.BlackboardReference.SetVariableValue("PlayerControl", v);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Lỗi khi thay đổi chế độ chỉ huy NPC: {ex.Message}");
            return false;
        }
    }

    public bool ReturnPlayerControlFromBlackBoard()
    {
        // Kiểm tra an toàn, phòng trường hợp BHA chưa được gán
        if (faBha == null)
        {
            return false;
        }

        bool isControlled;
        // Dùng GetVariableValue để đọc giá trị từ Blackboard
        if (faBha.BlackboardReference.GetVariableValue("PlayerControl", out isControlled))
        {
            // Nếu tìm thấy biến, trả về giá trị của nó
            return isControlled;
        }

        // Nếu không tìm thấy biến trên Blackboard, trả về false và báo lỗi
        Debug.LogWarning("Không tìm thấy biến 'PlayerControl' trên Blackboard!");
        return false;
    }

    #region  FaInterface
    // Hàm này cần được sửa lại cho đúng tên và tham số như trong Interface
    public void UseProtectiveAura(bool self) 
    {
        if (!IsSkillAvailable("ProtectiveAura")) return;
        // ... logic tạo lá chắn ...
        StartSkillCooldown("ProtectiveAura", 20f);
    }

    public void UseGuideSignal()
    {
        if (!IsSkillAvailable("GuideSignal")) return;
        Debug.Log("Thực thi kỹ năng TinHieuDanLoi (GuideSignal)!");
        StartSkillCooldown("GuideSignal", 10f);
    }

    public void UseKnowledgeLight()
    {
        if (!IsSkillAvailable("KnowledgeLight")) return;
        Debug.Log("Thực thi kỹ năng AnhSangTriThuc (KnowledgeLight)!");
        StartSkillCooldown("KnowledgeLight", 15f);
    }

    #endregion
}

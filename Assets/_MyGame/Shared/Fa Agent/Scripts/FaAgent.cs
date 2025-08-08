#nullable enable
using System.Collections.Generic;
using Code.UI.Gameplay;
using FMODUnity;
using Tu_Develop.Import.Scripts.EventConfig;
using Unity.Behavior;
using UnityEngine;

namespace Tu_Develop.Import.Scripts
{
    public class FaAgent : MonoBehaviour, FaInterface
    {
        [Header("Event Channels")] [SerializeField]
        private OnFaAgentUseSkill? useSkillEventChannel;
        [SerializeField] private FaAgentEventChannel? onReadyEventChannel;
        
        [Header("FMOD Sound Events")]
        [SerializeField] private EventReference guideSignalSfx;
        [SerializeField] private EventReference knowledgeLightSfx;
        [SerializeField] private EventReference protectiveAuraSfx;
        
        // Ambient sound
        [SerializeField] private EventReference ambientSound;

        [Header("UI Settings")]
        [SerializeField] private UIFaSkill? uiFaSkill;
        

        private readonly Dictionary<string, float> _cooldownTimers = new Dictionary<string, float>();

        // Bổ sung TaskQueue quản lý task cho Fa
        private readonly TaskQueue _taskQueue = new TaskQueue();
        // Hàm thêm task vào queue

        private void OnEnable()
        {
            if (useSkillEventChannel != null)
            {
                useSkillEventChannel.Event += OnSkillUsed;
            }
        }
        private void OnDisable()
        {
            if (useSkillEventChannel != null)
            {
                useSkillEventChannel.Event -= OnSkillUsed;
            }
        }
        private void AddTask(FaTask task)
        {
            _taskQueue.AddTask(task);
        }
        // Hàm lấy task tiếp theo
        private FaTask? GetNextTask()
        {
            return _taskQueue.GetNextTask();
        }
        // Kiểm tra còn task không
        private bool HasTask()
        {
            return _taskQueue.HasTask();
        }
        public bool IsSkillAvailable(string skillName)
        {
            return !_cooldownTimers.ContainsKey(skillName) || _cooldownTimers[skillName] <= 0;
        }

        public float ReturnCooldownSkill(string skillName)
        {
            // Trả về thời gian cooldown còn lại của skill, nếu không có thì trả về 0
            return _cooldownTimers.GetValueOrDefault(skillName, 0f);
        }
        // Hàm này sẽ được gọi từ Behavior Graph để bắt đầu đếm ngược
        private void StartSkillCooldown(string skillName, float duration)
        {
            _cooldownTimers[skillName] = duration;
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
                        var targetPos = new Vector3(x, y, z);
                        var task = new FaTask(TaskType.MoveTo)
                        {
                            TaskPosition = targetPos
                        };
                        AddTask(task);
                }
            }
            // SKILL
            else if (parts[0].ToLower() == "useskill" && parts.Length >= 2)
            {
                var skillName = parts[1]; // Skill name/number
            
                // Check cooldown
                if (!IsSkillAvailable(skillName))
                {
                    Debug.Log($"[FaAgent] Skill '{skillName}' đang hồi được {_cooldownTimers[skillName]}.");
                    return;
                }

                var task = new FaTask(TaskType.UseSkill)
                {
                    SkillName = skillName
                };

                if (parts.Length > 2)
                {
                    // Trường hợp có mục tiêu rõ ràng (vd: "player")
                    var targetKeyword = parts[2].ToLower();
                    task.TargetObject = (targetKeyword != "player"); // false nếu là player, true nếu khác
                    Debug.Log($"Đã thêm task UseSkill: {task.SkillName} trên mục tiêu self: {task.TargetObject}");
                }
                else
                {
                    // Trường hợp không có mục tiêu -> Mặc định là "self"
                    task.TargetObject = true;
                    Debug.Log($"Đã thêm task UseSkill: {task.SkillName} trên mục tiêu self: {task.TargetObject}");
                }
            
                            
                AddTask(task);
            }
            else
            {
                Debug.LogWarning("Lệnh không hợp lệ.");
            }
        }

        #region Audio Tasks

        /// <summary>
        /// Hàm này được gọi bởi EventChannel khi Cây Hành vi dùng node "Send Event Message".
        /// </summary>
        /// <param name="skillName">Đây là chuỗi string (tên skill) được gửi từ Cây Hành vi.</param>
        private void OnSkillUsed(string skillName)
        {
            Debug.Log($"[SỰ KIỆN LẮNG NGHE] Fa vừa sử dụng kỹ năng: {skillName}.");
            
            switch (skillName)
            {
                case "GuideSignal":
                    AudioManager.Instance?.PlayOneShot(guideSignalSfx, transform.position);
                    break;
                case "KnowledgeLight":
                    AudioManager.Instance?.PlayOneShot(knowledgeLightSfx, transform.position);
                    break;
                // ...
            }
        }

        #endregion
        
        
        public BehaviorGraphAgent? faBha;
        public BlackboardReference? playerInformationBlackboard;
        public BlackboardReference? idleConfigBlackboard;
        public DialogueConfig? idleConfigDialogue;
        public BlackboardReference? puzzleConfigBlackboard;
        public DialogueConfig? puzzleConfigDialogue;
        public BlackboardReference? combatConfigBlackboard;
        public DialogueConfig? combatConfigDialogue;
        private void Start()
        {
            faBha = GetComponent<BehaviorGraphAgent>();
            
            if (faBha && faBha.BlackboardReference != null)
            {
               // 1. Setup IdleConfig
               if (idleConfigBlackboard != null && idleConfigDialogue != null)
               {
                   // Set dialogue đầu tiên
                   var result = idleConfigBlackboard.SetVariableValue("Dialogues", idleConfigDialogue.dialogueSets[0].dialogues);
                   if (!result)
                   {
                       Debug.LogError("Không thể gán biến 'Dialogues' trên Blackboard!");
                   }
                   else
                   {
                       Debug.Log("Đã gán biến 'Dialogues' thành công.");
                   }
               }
               else
               {
                   Debug.LogWarning("Chưa gán IdleConfig Blackboard hoặc Dialogue!");
               }
               // 2. Setup PuzzleConfig
               // 3. Setup CombatConfig
               // 4. Setup PlayerInformation
                
                // Gán giá trị mặc định cho các biến trên Blackboard
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    faBha.BlackboardReference.SetVariableValue("Player", player);
                }
                

            }
            else
            {
                Debug.LogError("Không tìm thấy BehaviorGraphAgent trên đối tượng này!");
            }
            
            if (onReadyEventChannel != null)
            {
                onReadyEventChannel.RaiseEvent(this);
                Debug.Log("[FaAgent] Đã sẵn sàng và phát tín hiệu.");
            }
            else
            {
                Debug.LogWarning("Chưa gán Event Channel cho FaAgent!");
            }

            uiFaSkill = FindAnyObjectByType<UIFaSkill>();
        }

        void Update()
        {
            // Giảm cooldown mỗi frame
            var keys = new List<string>(_cooldownTimers.Keys);
            foreach (string key in keys)
            {
                _cooldownTimers[key] = Mathf.Max(0, _cooldownTimers[key] - Time.deltaTime);
                // đổi sang int
                var cooldownValue = Mathf.CeilToInt(_cooldownTimers[key]);
                uiFaSkill?.UpdateCoolDown(key, cooldownValue);
            }
        
            if (faBha == null) return;
            
            
            // 2. Đồng bộ hóa trạng thái cooldown LÊN Blackboard
            // Tên skill trong C# phải khớp với tên trong hàm IsSkillAvailable
            // Tên biến trên Blackboard phải khớp với những gì bạn đã tạo
            faBha.BlackboardReference.SetVariableValue("IsProtectiveAuraAvailable", IsSkillAvailable("ProtectiveAura"));
            faBha.BlackboardReference.SetVariableValue("IsGuideSignalAvailable", IsSkillAvailable("GuideSignal"));
            faBha.BlackboardReference.SetVariableValue("IsKnowledgeLightAvailable", IsSkillAvailable("KnowledgeLight"));
        
            // Chỉ xử lý task mới khi Graph rảnh và hàng đợi có task
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
                    else if (task.Type == TaskType.UseSkill)
                    {
                        // Dựa vào SkillName (hiện là "1", "2", "3" từ input) để chọn đúng enum
                        switch (task.SkillName)
                        {
                            case "GuideSignal":
                                playerTaskType = PlayerTaskType.GuideSignal;
                                UseGuideSignal();
                                break;
                            case "KnowledgeLight":
                                playerTaskType = PlayerTaskType.KnowledgeLight;
                                UseKnowledgeLight();
                                break;
                            case "ProtectiveAura":
                                playerTaskType = PlayerTaskType.ProtectiveAura;
                                UseProtectiveAura();
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
                    if (task.TaskPosition != null)
                        faBha.BlackboardReference.SetVariableValue("TaskPosition", task.TaskPosition);
                    faBha.BlackboardReference.SetVariableValue("TaskFromPlayer", playerTaskType);
                    if (task.TargetObject != null)
                        faBha.BlackboardReference.SetVariableValue("Self_ActiveProtectiveAura", task.TargetObject);
                    // Bật chế độ PlayerControl để Graph nhận task
                    //ActivePlayerControl(true);
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

            // Dùng GetVariableValue để đọc giá trị từ Blackboard
            if (faBha.BlackboardReference.GetVariableValue("PlayerControl", out bool isControlled))
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
        public void UseProtectiveAura() 
        {
            if (!IsSkillAvailable("ProtectiveAura")) return;
            StartSkillCooldown("ProtectiveAura", 20f);
            Code.Boss.BossEventSystem.Trigger(Code.Boss.BossEventType.FaSkillUsed, new Code.Boss.BossEventData("ProtectiveAura"));
        }

        public void UseGuideSignal()
        {
            if (!IsSkillAvailable("GuideSignal")) return;
            Debug.Log("Thực thi kỹ năng TinHieuDanLoi (GuideSignal)!");
            StartSkillCooldown("GuideSignal", 10f);
            Code.Boss.BossEventSystem.Trigger(Code.Boss.BossEventType.FaSkillUsed, new Code.Boss.BossEventData("GuideSignal"));
        }

        public void UseKnowledgeLight()
        {
            if (!IsSkillAvailable("KnowledgeLight")) return;
            Debug.Log("Thực thi kỹ năng AnhSangTriThuc (KnowledgeLight)!");
            StartSkillCooldown("KnowledgeLight", 15f);
            Code.Boss.BossEventSystem.Trigger(Code.Boss.BossEventType.FaSkillUsed, new Code.Boss.BossEventData("KnowledgeLight"));
        }

        #endregion
    }
}

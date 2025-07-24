using DuckLe;
using UnityEngine;

public class PlayerTaskInput : MonoBehaviour
{
    [Header("Player Components")]
    [SerializeField] private CharacterInput characterInput;
    
    [Header("Fa Components")]
    public FaAgent faAgent; // Kéo FaAgent vào đây trong Inspector
    private bool _isCommandMode;
    
    [Header("Skills Settings")]
    private bool _isWaitingForSkill3Target; // Cờ báo cho biết hệ thống đang chờ phím 1 hoặc 2
    private float _skill3PressTime; // Mốc thời gian khi người chơi nhấn phím 3
    private const float SKILL3_COMBO_TIMEOUT = 1.5f; // Thời gian tối đa để nhấn 1 hoặc 2 (1.5 giây)
    private const float SKILL3_HOLD_DURATION = 0.5f; // Thời gian cần giữ phím 3 để kích hoạt (0.5 giây)

    private void Start()
    {
        characterInput = GetComponent<CharacterInput>();
        // nếu chưa có faAgent thì tìm nó gắn vào nha
        if (faAgent == null)
        {
            faAgent = FindAnyObjectByType<FaAgent>();
            if (faAgent == null)
            {
                Debug.LogError("Không tìm thấy FaAgent trong scene. Vui lòng kéo FaAgent vào trường này trong Inspector.");
            }

            _isCommandMode = false;
        }
    }

    void Update()
    {
        // Thêm điều kiện để tránh bật tắt khi trong trạng thái đặt biệt
        // if (cutscene hay gi do) return;
        // Bật/tắt chế độ chỉ huy NPC
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            _isCommandMode = !_isCommandMode;
            faAgent.ActivePlayerControl(_isCommandMode);
            if (!_isCommandMode) _isWaitingForSkill3Target = false;
        }

        if (_isCommandMode == false) return;
        
        if (!faAgent.ReturnPlayerControlFromBlackBoard())
        {
            _isCommandMode = false;
            return;
        }
        
        // ƯU TIÊN 1: Nếu đang chờ combo của skill 3
        if (_isWaitingForSkill3Target)
        {
            // Nhấn phím 1 -> Target là Fa (Self)
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("[PlayerInput] Combo skill 3 -> Target: FA");
                faAgent.OnPlayerCommand("useskill ProtectiveAura");
                _isWaitingForSkill3Target = false;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log("[PlayerInput] Combo skill 2 -> Target: PLAYER");
                faAgent.OnPlayerCommand("useskill ProtectiveAura player");
                _isWaitingForSkill3Target = false;
            }

            if (Input.GetKey(KeyCode.Alpha3) && Time.time - _skill3PressTime > SKILL3_HOLD_DURATION)
            {
                Debug.Log("[PlayerInput] Giữ Skill 3 -> Target: FA");
                faAgent.OnPlayerCommand("useskill ProtectiveAura");
                _isWaitingForSkill3Target = false; // Kết thúc combo
            }
        }
        // ƯU TIÊN 2: Nếu không có combo nào đang chờ, lắng nghe input mới
        else
        {
            // Lệnh di chuyển
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 pos = characterInput.ReturnPointInput();
                faAgent.OnPlayerCommand($"move {pos.x} {pos.y} {pos.z}");
            }

            // Lệnh skill 1, 2
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                faAgent.OnPlayerCommand("useskill GuideSignal");
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                faAgent.OnPlayerCommand("useskill KnowledgeLight");
            }

            // Khi nhấn phím 3 lần đầu -> Bắt đầu trạng thái chờ combo
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Debug.Log("[PlayerInput] Bắt đầu combo Skill 3... Đang chờ phím 1 (Fa) hoặc 2 (Player)...");
                _isWaitingForSkill3Target = true;
                _skill3PressTime = Time.time; // Ghi lại thời điểm nhấn phím
            }
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = characterInput.ReturnPointInput(); // Trả ra toạ độ mun đến
            faAgent.OnPlayerCommand($"move {pos.x} {pos.y} {pos.z}");
        }
    }
}

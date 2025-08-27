using System;
using DuckLe;
using Tu_Develop.Import.Scripts;
using Tu_Develop.Import.Scripts.EventConfig;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class PlayerTaskInput : MonoBehaviour
{
    [Header("Player Components")]
    [SerializeField] private PlayerInput_02 characterInput;
    [SerializeField] private FaAgentEventChannel faAgentReadyChannel;
    [Header("Fa Components")]
    public FaAgent faAgent;
    private bool _isCommandMode;
    
    [Header("Skills Settings")]
    private bool _isWaitingForSkill3Target; // Cờ báo cho biết hệ thống đang chờ phím 1 hoặc 2
    private float _skill3PressTime; // Mốc thời gian khi người chơi nhấn phím 3
    private const float Skill3ComboTimeout = 1.5f; // Thời gian tối đa để nhấn 1 hoặc 2 (1.5 giây)
    private const float Skill3HoldDuration = 0.5f; // Thời gian cần giữ phím 3 để kích hoạt (0.5 giây)
    
    [SerializeField] private Volume postProcessVolume;
    private Vignette _vignette;

    bool isOke = false;


    // Dùng OnEnable và OnDisable để đăng ký và hủy đăng ký sự kiện
    //private void OnEnable()
    //{
    //    if (faAgentReadyChannel != null)
    //    {
    //        faAgentReadyChannel.OnFaAgentReady += SetFaAgentReference;
    //    }
    //    _isCommandMode = false;
    //}

    //private void OnDisable()
    //{
    //    if (faAgentReadyChannel != null)
    //    {
    //        faAgentReadyChannel.OnFaAgentReady -= SetFaAgentReference;
    //    }
    //}

    private void SetFaAgentReference(FaAgent agent)
    {
        faAgent = agent;
        Debug.Log("[PlayerTaskInput] Đã nhận được tham chiếu đến FaAgent!");
        
        postProcessVolume = GameObject.Find("Fa-Volume").GetComponent<Volume>();
        
        // Cố gắng lấy hiệu ứng Vignette từ profile của nó
        if (postProcessVolume != null && postProcessVolume.profile.TryGet(out Vignette vignette))
        {
            _vignette = vignette;
        }
        else
        {
            Debug.LogWarning("Không tìm thấy Global Volume hoặc Vignette trong scene!");
        }
        
    }

    private void LateUpdate()
    {

        if (isOke) return;

        if (faAgentReadyChannel != null)
        {
            faAgentReadyChannel.OnFaAgentReady += SetFaAgentReference;
        }
        _isCommandMode = false;

        isOke = true;
    }

    private void Update()
    {
        if (faAgent == null) return;
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            _isCommandMode = !_isCommandMode;
            faAgent.ActivePlayerControl(_isCommandMode);
            if (!_isCommandMode) _isWaitingForSkill3Target = false;
            
            // --- BỔ SUNG LOGIC VIGNETTE ---
            if (_vignette != null)
            {
                if (_isCommandMode)
                {
                    // Bật hiệu ứng khi vào chế độ chỉ huy
                    _vignette.intensity.value = 0.4f; // Hoặc giá trị bạn đã chọn
                }
                else
                {
                    // Tắt hiệu ứng khi thoát
                    _vignette.intensity.value = 0f;
                }
            }
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
            // SỬA LỖI 1: Thêm kiểm tra timeout
            if (Time.time - _skill3PressTime > Skill3ComboTimeout)
            {
                Debug.Log("[PlayerInput] Hết thời gian combo Skill 3.");
                _isWaitingForSkill3Target = false;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
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
            else if (Input.GetKey(KeyCode.Alpha3) && Time.time - _skill3PressTime > Skill3HoldDuration)
            {
                Debug.Log("[PlayerInput] Giữ Skill 3 -> Target: FA");
                faAgent.OnPlayerCommand("useskill ProtectiveAura");
                _isWaitingForSkill3Target = false; 
            }
        }
        // ƯU TIÊN 2: Nếu không có combo nào đang chờ, lắng nghe input mới
        else
        {
            // Lệnh di chuyển
            if (Input.GetMouseButtonDown(0))
            {
                var pos = characterInput.ReturnPointInput();
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
                _skill3PressTime = Time.time;
            }
        }
    }
}

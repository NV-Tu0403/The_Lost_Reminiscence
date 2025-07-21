using UnityEngine;

public class PlayerTaskInputExample : MonoBehaviour
{
    public FaAgent faAgent; // Kéo FaAgent vào đây trong Inspector
    private bool isCommandMode = false;

    private void Start()
    {
        // nếu chưa có faAgent thì tìm nó gắn vào nha
        if (faAgent == null)
        {
            faAgent = FindAnyObjectByType<FaAgent>();
            if (faAgent == null)
            {
                Debug.LogError("Không tìm thấy FaAgent trong scene. Vui lòng kéo FaAgent vào trường này trong Inspector.");
            }
        }
    }

    void Update()
    {
        // Bật/tắt chế độ chỉ huy NPC
        if (Input.GetKeyDown(KeyCode.F))
        {
            isCommandMode = !isCommandMode;
            Debug.Log(isCommandMode ? "[PlayerInput] bật chế độ chỉ huy NPC" : "[PlayerInput] tắt chế độ chỉ huy NPC");
            faAgent.ActivePlayerControl(isCommandMode);
        }

        if (isCommandMode)
        {
            //// Lệnh di chuyển bằng chuột trái
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Vector3 pos = hit.point;
                    faAgent.OnPlayerCommand($"move {pos.x} {pos.y} {pos.z}");
                }
            }
            // Lệnh dùng skill bằng phím số
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                faAgent.OnPlayerCommand("useskill EgoLight");
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                faAgent.OnPlayerCommand("useskill GuideSignal");
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                faAgent.OnPlayerCommand("useskill KnowledgeLight");
            }
            // Có thể mở rộng: skill cần vị trí thì raycast lấy tọa độ như trên
        }
    }
} 
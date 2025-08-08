using UnityEngine;

namespace Code.Boss.Testing
{
    [RequireComponent(typeof(CharacterController))]
    public class BasicPlayerMovement : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float mouseSensitivity = 100f;

        private CharacterController controller;
        private Transform cam;

        private float xRotation = 0f;

        void Start()
        {
            controller = GetComponent<CharacterController>();

            cam = Camera.main.transform;

            // Khóa con trỏ chuột
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            HandleMouseLook();
            HandleMovement();
        }

        void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // Xoay dọc (cam)
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // Xoay ngang (player body)
            transform.Rotate(Vector3.up * mouseX);
        }

        void HandleMovement()
        {
            float x = Input.GetAxis("Horizontal"); // A/D
            float z = Input.GetAxis("Vertical");   // W/S

            Vector3 move = transform.right * x + transform.forward * z;
            controller.Move(move * moveSpeed * Time.deltaTime);
        }
    }
}
//
// using UnityEngine;
// using UnityEngine.AI;
//
// [RequireComponent(typeof(NavMeshAgent))]
// public class PlayerNavMeshMovement : MonoBehaviour
// {
//     public float moveSpeed = 5f;
//
//     private NavMeshAgent agent;
//     private Transform mainCameraTransform;
//
//     void Start()
//     {
//         agent = GetComponent<NavMeshAgent>();
//         mainCameraTransform = Camera.main.transform;
//
//         // Đảm bảo agent không tự xoay nếu bạn điều khiển thủ công
//         agent.updateRotation = false;
//         agent.updateUpAxis = false;
//     }
//
//     void Update()
//     {
//         Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
//
//         if (input.magnitude >= 0.1f)
//         {
//             // Tính hướng theo camera
//             Vector3 cameraForward = mainCameraTransform.forward;
//             Vector3 cameraRight = mainCameraTransform.right;
//             cameraForward.y = 0f;
//             cameraRight.y = 0f;
//             cameraForward.Normalize();
//             cameraRight.Normalize();
//
//             // Hướng di chuyển
//             Vector3 moveDirection = cameraForward * input.y + cameraRight * input.x;
//             Vector3 destination = transform.position + moveDirection;
//
//             agent.speed = moveSpeed;
//             agent.SetDestination(destination);
//
//             // Xoay mặt nhân vật theo hướng di chuyển
//             if (moveDirection != Vector3.zero)
//             {
//                 Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
//                 transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
//             }
//         }
//         else
//         {
//             // Nếu không bấm phím thì không di chuyển
//             agent.SetDestination(transform.position);
//         }
//     }
// }

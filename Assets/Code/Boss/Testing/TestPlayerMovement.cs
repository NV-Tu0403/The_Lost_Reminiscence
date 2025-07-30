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
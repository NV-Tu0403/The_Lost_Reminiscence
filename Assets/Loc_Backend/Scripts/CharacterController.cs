using UnityEngine;

namespace Loc_Backend.Scripts
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterController : MonoBehaviour
    {
        public float moveSpeed = 5f;
        private UnityEngine.CharacterController controller;
        private Vector3 moveDirection;

        void Awake()
        {
            controller = GetComponent<UnityEngine.CharacterController>();
        }

        void Update()
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            moveDirection = new Vector3(h, 0, v);
            if (moveDirection.magnitude > 1f)
                moveDirection = moveDirection.normalized;
            controller.Move(transform.TransformDirection(moveDirection) * moveSpeed * Time.deltaTime);
        }
    }
}


using UnityEngine;

namespace _MyGame.Codes.Puzzle.Test
{
    public class MoveFa : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float moveDistance = 10f;

        [SerializeField] private Vector3 startPosition;
        [SerializeField] private Vector3 targetPosition;
        
        private bool isMoving = false;

        private void Start()
        {
            startPosition = transform.position;
            targetPosition = startPosition + new Vector3(moveDistance, 0, 0);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2) && !isMoving)
            {
                isMoving = true;
            }

            if (isMoving)
            {
                Move();
            }
        }

        private void Move()
        {
            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

            if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
            {
                transform.position = targetPosition; // Đảm bảo vị trí chính xác
                isMoving = false;
                
                // Swap positions để di chuyển ngược lại lần nhấn tiếp theo
                (startPosition, targetPosition) = (targetPosition, startPosition);
            }
        }
    }
}
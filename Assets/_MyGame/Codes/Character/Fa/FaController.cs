using TMPro;
using UnityEngine;

namespace Script.Character.Fa
{
    /// <summary>
    /// điều khiển nhân vật Fa
    /// </summary>
    public class FaController : MonoBehaviour
    {
        public Transform target;
        public Camera targetCamera; // Camera của target
        public float followDistance = 2f;
        public float moveSpeed = 5f; // Tốc độ di chuyển của Fa
        public float Heght = 2f; // Chiều cao của Fa
        public float cameraOffsetY = 1f; // Độ cao của Fa so với điểm nhìn của camera
        public float cameraOffsetZ = 2f; // Khoảng cách từ camera để đảm bảo trong tầm nhìn
        public float smoothTime = 0.3f; // Độ mượt của chuyển động
        private Vector3 velocity;

        public GameObject hintVFX;
        public GameObject puzzleUI;
        public TMP_Text dialogueText;

        [TextArea] public string[] hints;
        private int currentHint = 0;

        public Transform[] listsFollow;



        private void Start()
        {
            if (target == null)
            {
                FindTarget();
            }
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        void Update()
        {
            if (target == null)
            {
                FindTarget();
            }
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
            FlyFollowTarget();
        }

        private void FindTarget()
        {
            // Lọc GameObject trong Layer Character có tag Player
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject obj in players)
            {
                if (obj.layer == LayerMask.NameToLayer("Character"))
                {
                    target = obj.transform;
                    break;
                }
            }
        }

        /// <summary>
        /// Điều khiển nhân vật Fa đi theo mục tiêu
        /// </summary>
        //void FollowTarget()
        //{
        //    if (navMeshAgent != null && Vector3.Distance(transform.position, target.position) > followDistance)
        //    {
        //        navMeshAgent.SetDestination(target.position);
        //    }
        //    else if (navMeshAgent != null)
        //    {
        //        navMeshAgent.ResetPath();
        //    }
        //}

        void FlyFollowTarget()
        {
            if (target == null || targetCamera == null) return;

            // === 1. Tính vị trí mục tiêu theo camera ===
            Vector3 cameraForward = targetCamera.transform.forward;
            Vector3 cameraPosition = targetCamera.transform.position;

            Vector3 targetPosition = cameraPosition + cameraForward * cameraOffsetZ + Vector3.up * cameraOffsetY;

            // === 2. Kiểm tra Fa có trong khung hình không ===
            Vector3 viewportPoint = targetCamera.WorldToViewportPoint(transform.position);
            bool isInView = viewportPoint.z > 0 && viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1;

            // === 3. Lấy hiệu ứng dao động (nếu cần) ===
            //Vector3 bobbingOffset = CalculateBobbingOffset(isInView);

            // === 4. Tính khoảng cách và điều chỉnh tốc độ ===
            float distance = Vector3.Distance(transform.position, targetPosition);
            float currentSmoothTime = smoothTime;
            float currentMoveSpeed = moveSpeed;

            //if (distance > followDistance * 2f)
            //{
            //    currentMoveSpeed *= 1.5f;
            //}

            // === 5. Di chuyển nếu cần thiết ===
            if (!isInView || distance > followDistance)
            {
                Vector3 smoothedPosition = Vector3.SmoothDamp(
                    transform.position,
                    targetPosition,
                    ref velocity,
                    currentSmoothTime,
                    currentMoveSpeed
                );

                transform.position = smoothedPosition /*+ bobbingOffset*/;
            }
            //else
            //{
            //    transform.position += bobbingOffset;
            //}

            // === 6. Xoay Fa về phía Player ===
            Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }


        Vector3 CalculateBobbingOffset(bool isInView)
        {
            if (!isInView) return Vector3.zero;

            float bobAmplitude = 0.05f;
            float bobFrequency = 2f;

            float bobOffsetX = Mathf.Cos(Time.time * bobFrequency) * bobAmplitude;
            float bobOffsetY = Mathf.Sin(Time.time * bobFrequency * 1.2f) * bobAmplitude * 0.5f;

            return new Vector3(bobOffsetX, bobOffsetY, 0f);
        }


        /// <summary>
        /// Hiện hiệu ứng gợi ý 
        /// </summary>
        public void ShowHint()
        {
            if (currentHint < hints.Length)
            {
                dialogueText.text = hints[currentHint];
                currentHint++;
            }
        }

        /// <summary>
        /// Điều khiển nhân vật Fa đi theo một mục tiêu cụ thể
        /// </summary>
        public void PointToNote()
        {
        }

        /// <summary>
        /// Mở giao diện trò chơi ghép hình
        /// </summary>
        public void OpenPuzzleUI()
        {
            puzzleUI.SetActive(true);
        }

        /// <summary>
        /// Đóng giao diện trò chơi ghép hình
        /// </summary>
        /// <param name="correct"></param>
        public void ReactToAnswer(bool correct)
        {
            if (correct)
                dialogueText.text = "Tuyệt lắm! Đó là nốt đúng!";
            else
                dialogueText.text = "Không phải nốt này, thử lại nhé.";
        }
    }
}

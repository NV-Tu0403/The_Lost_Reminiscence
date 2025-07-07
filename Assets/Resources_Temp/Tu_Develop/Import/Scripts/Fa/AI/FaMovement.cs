using UnityEngine;
using UnityEngine.AI;

namespace Fa.AI
{
    public enum FaMovementMode
    {
        NavMesh,
        Basic,
        Flying
    }

    /// <summary>
    /// Implementation cho hệ thống di chuyển của Fa với NavMesh, basic và flying mode
    /// </summary>
    public class FaMovement : MonoBehaviour, IFaMovement
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float stoppingDistance = 0.1f;
        [SerializeField] private bool useNavMesh = true;
        [SerializeField] private float navMeshSampleRadius = 5f;
        [Header("Flying Settings")]
        [SerializeField] private float flyingSpeed = 4f;
        [SerializeField] private float flyingHeight = 2f;
        [SerializeField] private float flyingArriveDistance = 0.05f;
        [Header("Fallback Settings")]
        [SerializeField] private float basicMovementSpeed = 2f;
        [SerializeField] private float stuckDetectionTime = 3f;
        [SerializeField] private float stuckDetectionDistance = 0.1f;

        private Vector3 targetPosition;
        private bool isMoving = false;
        private Transform currentTarget;
        private NavMeshAgent navMeshAgent;
        private MovementState currentState = MovementState.Idle;
        private Vector3 lastPosition;
        private float stuckTimer;
        private bool isNavMeshAvailable = false;
        private FaMovementMode currentMode = FaMovementMode.NavMesh;

        public FaMovementMode CurrentMode => currentMode;

        #region IFaMovement Implementation

        public void MoveTo(Vector3 targetPosition)
        {
            this.targetPosition = targetPosition;
            this.currentTarget = null;
            isMoving = true;

            // Ưu tiên NavMesh nếu có, nếu không thì flying
            if (useNavMesh && isNavMeshAvailable)
            {
                UseNavMeshMovement(targetPosition);
                currentMode = FaMovementMode.NavMesh;
            }
            else if (!isNavMeshAvailable)
            {
                UseFlyingMovement(targetPosition);
                currentMode = FaMovementMode.Flying;
            }
            else
            {
                UseBasicMovement(targetPosition);
                currentMode = FaMovementMode.Basic;
            }
        }

        public void FollowPlayer(Transform playerTransform, float followDistance)
        {
            if (playerTransform == null) return;

            currentTarget = playerTransform;
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer > followDistance)
            {
                Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
                Vector3 followPosition = playerTransform.position - directionToPlayer * (followDistance * 0.8f);

                if (useNavMesh && isNavMeshAvailable)
                {
                    UseNavMeshMovement(followPosition);
                    currentMode = FaMovementMode.NavMesh;
                }
                else if (!isNavMeshAvailable)
                {
                    UseFlyingMovement(followPosition);
                    currentMode = FaMovementMode.Flying;
                }
                else
                {
                    UseBasicMovement(followPosition);
                    currentMode = FaMovementMode.Basic;
                }

                isMoving = true;
                currentState = MovementState.Following;
            }
            else
            {
                StopMoving();
            }
        }

        public void StopMoving()
        {
            isMoving = false;
            currentTarget = null;
            currentState = MovementState.Idle;

            if (navMeshAgent != null)
            {
                navMeshAgent.isStopped = true;
            }
        }

        public bool IsMoving => isMoving;

        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = value;
        }

        public float DistanceToTarget
        {
            get
            {
                if (currentTarget != null)
                {
                    return Vector3.Distance(transform.position, currentTarget.position);
                }
                return Vector3.Distance(transform.position, targetPosition);
            }
        }

        public bool IsNavMeshAvailable => isNavMeshAvailable;

        public void SetMovementMode(bool useNavMesh)
        {
            this.useNavMesh = useNavMesh;
            Debug.Log($"Fa Movement Mode: {(useNavMesh ? "NavMesh" : "Basic/Flying")}");
        }

        public MovementState CurrentState => currentState;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            InitializeNavMeshAgent();
            CheckNavMeshAvailability();
        }

        private void Start()
        {
            lastPosition = transform.position;
        }

        private void Update()
        {
            if (!isMoving) return;

            // Cập nhật target position nếu đang follow player
            if (currentTarget != null)
            {
                FollowPlayer(currentTarget, 2f);
            }

            // Kiểm tra stuck
            CheckIfStuck();

            // Cập nhật movement
            UpdateMovement();
        }

        #endregion

        #region Private Methods

        private void InitializeNavMeshAgent()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            if (navMeshAgent == null)
            {
                navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
            }

            navMeshAgent.speed = moveSpeed;
            navMeshAgent.stoppingDistance = stoppingDistance;
            navMeshAgent.radius = 0.5f;
            navMeshAgent.height = 2f;
            navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        }

        private void CheckNavMeshAvailability()
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
            {
                isNavMeshAvailable = true;
            }
            else
            {
                isNavMeshAvailable = false;
            }
        }

        private void UseNavMeshMovement(Vector3 target)
        {
            if (navMeshAgent != null && isNavMeshAvailable)
            {
                navMeshAgent.enabled = true;
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(target);
                currentState = MovementState.Pathfinding;
            }
            else
            {
                UseFlyingMovement(target);
            }
        }

        private void UseBasicMovement(Vector3 target)
        {
            targetPosition = target;
            currentState = MovementState.Moving;
        }

        private void UseFlyingMovement(Vector3 target)
        {
            targetPosition = target;
            currentState = MovementState.Moving;
            if (navMeshAgent != null) navMeshAgent.enabled = false;
        }

        private void UpdateMovement()
        {
            if (currentMode == FaMovementMode.NavMesh && currentState == MovementState.Pathfinding)
            {
                if (navMeshAgent != null && navMeshAgent.remainingDistance <= stoppingDistance)
                {
                    StopMoving();
                }
            }
            else if (currentMode == FaMovementMode.Basic && currentState == MovementState.Moving)
            {
                MoveTowardsTargetBasic();
            }
            else if (currentMode == FaMovementMode.Flying && currentState == MovementState.Moving)
            {
                MoveTowardsTargetFlying();
            }
        }

        private void MoveTowardsTargetBasic()
        {
            if (Vector3.Distance(transform.position, targetPosition) <= stoppingDistance)
            {
                StopMoving();
                return;
            }

            Vector3 direction = (targetPosition - transform.position).normalized;
            Vector3 movement = direction * basicMovementSpeed * Time.deltaTime;

            transform.position += movement;

            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        private void MoveTowardsTargetFlying()
        {
            Vector3 flyTarget = targetPosition;
            flyTarget.y = flyingHeight;
            Vector3 current = transform.position;
            current.y = flyingHeight;

            if (Vector3.Distance(current, flyTarget) <= flyingArriveDistance)
            {
                StopMoving();
                return;
            }

            Vector3 direction = (flyTarget - current).normalized;
            Vector3 movement = direction * flyingSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(current, flyTarget, flyingSpeed * Time.deltaTime);

            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        private void CheckIfStuck()
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);

            if (distanceMoved < stuckDetectionDistance)
            {
                stuckTimer += Time.deltaTime;

                if (stuckTimer > stuckDetectionTime)
                {
                    currentState = MovementState.Stuck;
                    Debug.LogWarning("Fa: Bị kẹt! Thử tìm đường khác...");

                    if (useNavMesh && isNavMeshAvailable)
                    {
                        Vector3 randomOffset = Random.insideUnitSphere * 2f;
                        randomOffset.y = 0;
                        Vector3 newTarget = transform.position + randomOffset;

                        if (NavMesh.SamplePosition(newTarget, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                        {
                            UseNavMeshMovement(hit.position);
                        }
                        else
                        {
                            SetMovementMode(false);
                            UseFlyingMovement(targetPosition);
                        }
                    }
                    else
                    {
                        Vector3 randomDirection = Random.insideUnitSphere.normalized;
                        randomDirection.y = 0;
                        Vector3 newTarget = transform.position + randomDirection * 2f;
                        UseFlyingMovement(newTarget);
                    }

                    stuckTimer = 0f;
                }
            }
            else
            {
                stuckTimer = 0f;
            }

            lastPosition = transform.position;
        }

        #endregion

        #region Debug Methods

        private void OnDrawGizmosSelected()
        {
            if (isMoving)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, targetPosition);
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(targetPosition, 0.5f);
            }

            if (currentTarget != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(currentTarget.position, 2f);
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, navMeshSampleRadius);

            if (currentState == MovementState.Stuck)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, stuckDetectionDistance);
            }
        }

        private void OnGUI()
        {
            if (Application.isPlaying)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
                screenPos.y = Screen.height - screenPos.y;
                
                string debugText = $"Fa Movement State: {currentState}\n" +
                                 $"NavMesh: {(isNavMeshAvailable ? "Available" : "Not Available")}\n" +
                                 $"Mode: {(useNavMesh ? "NavMesh" : "Basic/Flying")}\n" +
                                 $"Distance: {DistanceToTarget:F2}";
                
                GUI.Label(new Rect(screenPos.x, screenPos.y, 200, 80), debugText);
            }
        }

        #endregion
    }
} 
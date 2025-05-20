using UnityEngine;
//using Fusion;
using DuckLe;

[System.Serializable]
public struct AnimationCCData
{
    public bool IsStrafing;
    public bool Grounded;
    public float GroundDistance;
    public float HorizontalSpeed;
    public float VerticalSpeed;
    public float InputMagnitude;
    public bool IsMeleeAttacking;
    public bool IsThrowing;
}

namespace DuckLe
{
    [RequireComponent(typeof(Animator), typeof(PlayerController))]
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] public MovementSpeed freeSpeed = new MovementSpeed();
        [SerializeField] public MovementSpeed strafeSpeed = new MovementSpeed();

        private Animator animator;
        private PlayerController _pc;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.updateMode = AnimatorUpdateMode.Fixed;
            _pc = GetComponent<PlayerController>();
        }

        private void Update()
        {
            AnimationCCData data = _pc.GetAnimationData();
            UpdateAnimator(data);
        }

        public void UpdateAnimator(AnimationCCData data)
        {
            if (animator == null || !animator.enabled) return;

            animator.SetBool(AnimatorParameters.IsStrafing, data.IsStrafing);
            animator.SetBool(AnimatorParameters.IsGrounded, data.Grounded);
            animator.SetFloat(AnimatorParameters.GroundDistance, data.GroundDistance);
            animator.SetBool(AnimatorParameters.IsMeleeAttacking, data.IsMeleeAttacking);
            animator.SetBool(AnimatorParameters.IsThrowing, data.IsThrowing);

            float smoothTime = data.IsStrafing ? strafeSpeed.animationSmooth : freeSpeed.animationSmooth;
            animator.SetFloat(AnimatorParameters.InputHorizontal, data.HorizontalSpeed, smoothTime, Time.deltaTime);
            animator.SetFloat(AnimatorParameters.InputVertical, data.VerticalSpeed, smoothTime, Time.deltaTime);
            animator.SetFloat(AnimatorParameters.InputMagnitude, data.InputMagnitude, smoothTime, Time.deltaTime);
        }

        [System.Serializable]
        public class MovementSpeed
        {
            [Range(1f, 20f)]
            public float movementSmooth = 6f;
            [Range(0f, 1f)]
            public float animationSmooth = 0.2f;
            public float rotationSpeed = 16f;
            public bool walkByDefault = false;
            public bool rotateWithCamera = false;
            public float walkSpeed = 2f;
            public float runningSpeed = 4f;
            public float sprintSpeed = 8f; // Khớp với sprintSpeed của PlayerController
        }
    }

    public static partial class AnimatorParameters
    {
        public static int InputHorizontal = Animator.StringToHash("InputHorizontal");
        public static int InputVertical = Animator.StringToHash("InputVertical");
        public static int InputMagnitude = Animator.StringToHash("InputMagnitude");
        public static int IsGrounded = Animator.StringToHash("IsGrounded");
        public static int IsStrafing = Animator.StringToHash("IsStrafing");
        public static int GroundDistance = Animator.StringToHash("GroundDistance");
        public static int IsMeleeAttacking = Animator.StringToHash("IsMeleeAttacking");
        public static int IsThrowing = Animator.StringToHash("IsThrowing");
    }
}
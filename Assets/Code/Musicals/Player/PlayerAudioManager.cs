using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class PlayerAudioManager : MonoBehaviour
{
    [field: Header("Player SFX")]
    [field: SerializeField] public EventReference walk_FootstepReference {get; private set;}
    [field: SerializeField] public EventReference run_FootstepReference { get; private set;}

    public static PlayerAudioManager Instance {get; private set;}

    [field: SerializeField] public EventInstance walk_FootstepInstance { get; private set;}
    [field: SerializeField] public EventInstance run_FootstepInstance { get; private set; }

    private Rigidbody playerRigidbody;
    private bool isFootstepPlaying = false;

    private enum FootstepState { Idle, Walk, Run }
    private FootstepState currentFootstepState = FootstepState.Idle;

    void Awake()
    {
        walk_FootstepInstance = RuntimeManager.CreateInstance(walk_FootstepReference);
        walk_FootstepInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));

        run_FootstepInstance = RuntimeManager.CreateInstance(run_FootstepReference);
        run_FootstepInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple instances of PlayerAudioManager detected. Destroying duplicate instance.");
        }

    }

    private void Start()
    {
        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponentInParent<Rigidbody>();
            if (playerRigidbody == null)
            {
                Debug.LogError("PlayerAudioManager requires a Rigidbody component on the player GameObject.");
            }
        }
    }

    void Update()
    {
        float speed = playerRigidbody.linearVelocity.magnitude;
        if (speed > 1f)
        {
            SetFootstepState(FootstepState.Run);
        }
        else if (speed > 0.1f)
        {
            SetFootstepState(FootstepState.Walk);
        }
        else
        {
            SetFootstepState(FootstepState.Idle);
        }
    }

    private void SetFootstepState(FootstepState newState)
    {
        if (currentFootstepState == newState) return;
        currentFootstepState = newState;
        switch (newState)
        {
            case FootstepState.Run:
                if (!isFootstepPlaying)
                {
                    run_FootstepInstance.start();
                    isFootstepPlaying = true;
                }
                run_FootstepInstance.setParameterByName("FootstepType", 1); // 1 = Run
                break;
            case FootstepState.Walk:
                if (!isFootstepPlaying)
                {
                    walk_FootstepInstance.start();
                    isFootstepPlaying = true;
                }
                walk_FootstepInstance.setParameterByName("FootstepType", 0); // 0 = Walk
                break;
            case FootstepState.Idle:
                if (isFootstepPlaying)
                {
                    walk_FootstepInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                    run_FootstepInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                    isFootstepPlaying = false;
                }
                break;
        }
    }
}

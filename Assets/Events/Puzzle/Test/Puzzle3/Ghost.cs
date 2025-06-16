using Events.Puzzle.Test.Puzzle3;
using UnityEngine;

public enum GhostState { Idle, Talk, Chase }

public class Ghost : MonoBehaviour
{
    [Header("Ghost Settings")]
    public float chaseSpeed = 3f;
    public float talkDistance = 2f;
    public GameObject chatBubblePrefab;

    private GhostState state = GhostState.Idle;
    private Transform player;
    private GameObject chatBubbleInstance;
    private bool hasTalked = false;
    private bool isChasing = false;
    private GhostZone zone;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    void Start()
    {
        zone = GetComponentInParent<GhostZone>();
    }

    void Update()
    {
        if (state == GhostState.Chase && player != null)
        {
            // Nếu player bị dịch chuyển ra xa khỏi ghost zone, dừng chase
            float maxChaseDistance = 10f; // Có thể điều chỉnh phù hợp với zone của bạn
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist > maxChaseDistance)
            {
                StopChase();
                return;
            }
            // Đuổi theo player
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * chaseSpeed * Time.deltaTime;
        }
        else if (state == GhostState.Talk && player != null)
        {
            // Nếu player rời xa, ẩn chat bubble
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist > talkDistance)
            {
                HideChatBubble();
                state = GhostState.Idle;
                player = null;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Không gán player ở đây nữa, chỉ xử lý chat bubble và talk logic
            if (state == GhostState.Chase && zone != null)
            {
                zone.OnGhostHitPlayer();
            }
            if (!hasTalked)
            {
                ShowChatBubble();
                state = GhostState.Talk;
                hasTalked = true;
                // Gọi về GhostZone để tăng talkCount
                if (zone != null)
                    zone.OnTalkToGhost(this);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HideChatBubble();
            state = GhostState.Idle;
            player = null;
        }
    }

    public void StartChase()
    {
        state = GhostState.Chase;
        HideChatBubble();
        isChasing = true;
    }

    public void StopChase()
    {
        state = GhostState.Idle;
        isChasing = false;
        player = null;
    }

    public void SetTarget(Transform target)
    {
        player = target;
    }

    private void ShowChatBubble()
    {
        if (chatBubblePrefab != null && chatBubbleInstance == null)
        {
            Vector3 bubblePosition = transform.position + Vector3.up * 2f;
            chatBubbleInstance = Instantiate(chatBubblePrefab, bubblePosition, Quaternion.identity, transform);

            var chatBubble = chatBubbleInstance.GetComponent<ChatBubble>();
            if (chatBubble != null)
            {
                chatBubble.SetMessage("...");
            }
        }
    }


    private void HideChatBubble()
    {
        if (chatBubbleInstance != null)
        {
            Destroy(chatBubbleInstance);
            chatBubbleInstance = null;
        }
    }

    public void ResetState()
    {
        state = GhostState.Idle;
        isChasing = false;
        hasTalked = false;
        player = null;
        HideChatBubble();
        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }
}
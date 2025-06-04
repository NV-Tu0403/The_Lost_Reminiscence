using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody _rb;
    private Vector3 _movement;
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }
    
    private void Update()
    {
        // Get input from the user
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Create a movement vector based on input
        _movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
    }
    
    private void FixedUpdate()
    {
        // Move the player
        _rb.MovePosition(transform.position + _movement * moveSpeed * Time.fixedDeltaTime);
    }
}

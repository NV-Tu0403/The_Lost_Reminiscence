using UnityEngine;

public class FloatMovement : MonoBehaviour
{
    public float floatStrength = 1f; 
    public float speed = 2f;           

    private Vector3 startPos;
        
    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.position = startPos + new Vector3(0, Mathf.Sin(Time.time * speed) * floatStrength, 0);
    }
}

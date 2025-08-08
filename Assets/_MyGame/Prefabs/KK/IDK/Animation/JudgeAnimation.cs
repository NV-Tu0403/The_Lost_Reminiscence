using UnityEngine;

public class JudgeAnimation : MonoBehaviour
{
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.Play("DungCho");  
    }
}


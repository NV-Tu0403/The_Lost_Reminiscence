using UnityEngine;
using System.Collections;

public class DialogueChat : MonoBehaviour
{
    private Camera mainCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void OnEnable()
    {
        StartCoroutine(Initialize());
    }

    IEnumerator Initialize()
    {
        while (mainCamera == null)
        {
            mainCamera = Camera.main;
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (mainCamera != null)
        {
            Vector3 direction = this.gameObject.transform.position - mainCamera.transform.position;
            this.gameObject.transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }
}

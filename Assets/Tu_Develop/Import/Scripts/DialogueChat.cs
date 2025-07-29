using UnityEngine;
using System.Collections;

public class DialogueChat : MonoBehaviour
{
    private Camera _mainCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnEnable()
    {
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        while (_mainCamera == null)
        {
            _mainCamera = Camera.main;
            yield return null;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (_mainCamera != null)
        {
            Vector3 direction = this.gameObject.transform.position - _mainCamera.transform.position;
            this.gameObject.transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}

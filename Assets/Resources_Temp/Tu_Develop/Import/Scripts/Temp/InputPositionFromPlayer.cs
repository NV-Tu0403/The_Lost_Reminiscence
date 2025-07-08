using UnityEngine;
using System.Collections;
public class InputPositionFromPlayer : MonoBehaviour
{
    public bool state = false;
    public LayerMask groundLayer;
    private GameObject previewCube;

    private Vector3 positionFromPlayer = Vector3.zero;

    public System.Action<Vector3> OnPositionSelected; // Callback trả về pos

    void Start()
    {
        previewCube = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube), Vector3.zero, Quaternion.identity);
        previewCube.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (state) return;
            state = true;
            StartCoroutine(PreviewPostionRoutine());
        }
    }

    IEnumerator PreviewPostionRoutine()
    {
        previewCube.SetActive(true);
        while (state)
        {
            // Lấy ray từ camera qua vị trí chuột
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f, groundLayer))
            {
                positionFromPlayer = hit.point;
                previewCube.transform.position = positionFromPlayer;
            }

            // Nếu nhấn chuột trái, xác nhận vị trí
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                state = false;
                Debug.Log($"Position Selected: {positionFromPlayer}");
                OnPositionSelected?.Invoke(positionFromPlayer);
            }

            yield return null;
        }
        previewCube.SetActive(false);
    }
}
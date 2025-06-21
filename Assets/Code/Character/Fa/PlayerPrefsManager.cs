using TMPro;
using UnityEngine;

public class PlayerPrefsManager : MonoBehaviour
{
    [SerializeField] Transform obj;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            SaveValue();
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            UploadValue();
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            DeleteData();
        }
    }
    public void SaveValue()
    {
        Vector3 pos = new Vector3(obj.position.x, obj.position.y, obj.position.z);

        PlayerPrefs.SetFloat("PosX", pos.x);
        PlayerPrefs.SetFloat("PosY", pos.y);
        PlayerPrefs.SetFloat("PosZ", pos.z);

        PlayerPrefs.Save();

        Debug.Log("Luu thanh cong");
    }

    public void UploadValue()
    {
        Vector3 pos = new Vector3(PlayerPrefs.GetFloat("PosX"), PlayerPrefs.GetFloat("PosY"), PlayerPrefs.GetFloat("PosZ"));
        obj.position = pos;
        Debug.Log("Tai thanh cong");
    }

    public void DeleteData()
    {
        PlayerPrefs.DeleteAll();
    }
}
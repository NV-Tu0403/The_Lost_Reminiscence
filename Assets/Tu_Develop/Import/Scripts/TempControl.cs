using Unity.Behavior;
using UnityEngine;

public class TempControl : MonoBehaviour
{
    [SerializeField] private BehaviorGraphAgent faBG;

    public GameObject targetInput;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (faBG != null)
            {
                faBG.SetVariableValue("Input Target From Player", targetInput.transform);
            }
            else
            {
                Debug.LogWarning("BehaviorGraphAgent is not assigned.");
            }
        }
    }


}

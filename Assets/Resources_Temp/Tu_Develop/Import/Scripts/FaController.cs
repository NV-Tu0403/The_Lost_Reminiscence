using Unity.Behavior;
using UnityEngine;

public class FaController : MonoBehaviour
{
    [SerializeField] private BehaviorGraphAgent mainControl;
    void Start()
    {
        if (mainControl != null)
        {
            // Gắn MainTransform vào thuộc tính GameObject Kien cua mainControl
            var mainGameObject = GameObject.Find("Player");
            mainControl.SetVariableValue("Main Character", mainGameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

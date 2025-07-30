using UnityEngine;

public class MapStateObject : MonoBehaviour
{
    [Tooltip("Mỗi object trong map cần có ID riêng biệt!")]
    public string UniqueID;

    public bool trackActive = true;
    public bool trackTransform = true;

    public MapObjectState CaptureState()
    {
        var state = new MapObjectState
        {
            id = UniqueID
        };

        if (trackActive)
        {
            state.hasIsActive = true;
            state.isActive = gameObject.activeSelf;
        }

        if (trackTransform)
        {
            state.hasPosition = true;
            state.position = transform.position;

            state.hasRotation = true;
            state.rotation = transform.rotation.eulerAngles;
        }

        return state;
    }


    public void ApplyState(MapObjectState state)
    {
        if (state.hasIsActive)
            gameObject.SetActive(state.isActive);

        if (state.hasPosition)
            transform.position = state.position;

        if (state.hasRotation)
            transform.rotation = Quaternion.Euler(state.rotation);
    }

}

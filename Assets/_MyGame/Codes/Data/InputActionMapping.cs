using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "InputActionMapping", menuName = "Data/InputActionMapping", order = 1)]
public class InputActionMapping : ScriptableObject
{
    [System.Serializable]
    public struct KeyStateAction
    {
        public CoreStateType state;
        public List<UIActionType> actions;
    }

    [System.Serializable]
    public struct KeyActionMap
    {
        public KeyCoreInputType key;
        public List<KeyStateAction> stateActions;
    }

    [SerializeField]
    private List<KeyActionMap> mappings;

    public bool TryGetActions(KeyCoreInputType key, CoreStateType state, out List<UIActionType> actions)
    {
        actions = null;
        foreach (var mapping in mappings)
        {
            if (mapping.key == key)
            {
                foreach (var sa in mapping.stateActions)
                {
                    if (sa.state == state)
                    {
                        actions = sa.actions;
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
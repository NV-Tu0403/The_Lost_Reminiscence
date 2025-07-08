using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "InputActionMapping", menuName = "Data/InputActionMapping", order = 1)]
public class InputActionMapping : ScriptableObject
{
    [System.Serializable]
    public struct KeyActionPair
    {
        public KeyCoreInputType key;
        public List<UIActionType> actions;
    }

    [SerializeField]
    private List<KeyActionPair> mappings = new List<KeyActionPair>();

    // Phương thức để lấy danh sách hành động cho một key
    public bool TryGetActions(KeyCoreInputType key, out List<UIActionType> actions)
    {
        actions = null;
        foreach (var mapping in mappings)
        {
            if (mapping.key == key)
            {
                actions = mapping.actions;
                return true;
            }
        }
        return false;
    }

    // Phương thức để lấy toàn bộ ánh xạ (nếu cần)
    public List<KeyActionPair> GetAllMappings()
    {
        return mappings;
    }
}
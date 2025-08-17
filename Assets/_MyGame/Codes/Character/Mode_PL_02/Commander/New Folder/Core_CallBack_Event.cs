using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Core_CallBack_Event", menuName = "Configs/Core_CallBack_Event", order = 1)]
public class Core_CallBack_Event : ScriptableObject
{
    [SerializeField] private List<CutSceneGroup> cutSceneGroups;

    [System.Serializable]
    public struct CutSceneGroup
    {
        public UIActionType actionType;
        public CutSceneEvent cutSceneEvent;
    }

    public CutSceneEvent GetCutSceneEventByActionType(UIActionType actionType)
    {
        foreach (var group in cutSceneGroups)
        {
            if (group.actionType == actionType)
            {
                return group.cutSceneEvent;
            }
        }
        Debug.LogWarning($"No CutSceneEvent found for action type: {actionType}");
        return default;
    }
}

[System.Serializable]
public struct CutScenwItem
{
    public GameObject cutSceneObject;
    public int order;
    public float duration;
    [SerializeField] public LogicPoint[] logicPoints;
}

[System.Serializable]
public struct LogicPoint
{
    public float timePoint; // Thời điểm thực thi (0-1, tương ứng 0-100% duration)
    public UnityEvent customAction;
}

[System.Serializable]
public struct CutSceneEvent
{
    public CutScenwItem[] cutSceneItems;
}
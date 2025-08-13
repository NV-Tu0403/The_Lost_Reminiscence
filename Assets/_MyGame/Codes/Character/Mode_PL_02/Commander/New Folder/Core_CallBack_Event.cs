using System;
using UnityEngine.Events;
using UnityEngine;

[Serializable]
public struct CutScenwItem
{
    public GameObject cutSceneObject;
    public int order;
    public float duration;
    public LogicPoint[] logicPoints;
}

[Serializable]
public struct LogicPoint
{
    public float timePoint; // Thời điểm thực thi (0-1, tương ứng 0-100% duration)
    public UnityEvent customAction;
}

[Serializable]
public struct CutSceneEvent
{
    public CutScenwItem[] cutSceneItems;
}

[CreateAssetMenu(fileName = "Core_CallBack_Event", menuName = "Configs/Core_CallBack_Event", order = 1)]
public class Core_CallBack_Event : ScriptableObject
{
    [SerializeField] private CutSceneEvent cutSceneEvent;

    public CutScenwItem GetCutSceneItemByOrder(int order)
    {
        foreach (var item in cutSceneEvent.cutSceneItems)
        {
            if (item.order == order)
            {
                return item;
            }
        }
        Debug.LogWarning($"No CutSceneItem found for order: {order}");
        return default;
    }
}
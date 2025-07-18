using UnityEngine;
using System.Collections.Generic;
using Code.Procession;
using Script.Procession.Conditions;
using Script.Procession.Reward.ScriptableObjects;

// ScriptableObject tổng hợp chứa tất cả MainProcess
[CreateAssetMenu(fileName = "ProgressionData", menuName = "Progression/ProgressionData")]
public class ProgressionDataSO : ScriptableObject
{
    public List<MainProcessSO> MainProcesses;
    
    /// <summary>
    /// Chuyển từ SO → đối tượng GameProgression
    /// </summary>
    public GameProgression ToGameProgression()
    {
        var progression = new GameProgression
        {
            mainProcesses = MainProcesses.ConvertAll(so => so.ToMainProcess())
        };
        // Debug.Log($"[ProgressionDataSO] Converted {progression.MainProcesses.Count} MainProcesses");
        return progression;
    }
}

// ScriptableObject cho MainProcess
[System.Serializable]
public class MainProcessSO
{
    public string Id;
    
    public MainProcess.ProcessType Type; 

    public string Name;
    public string Description;
    public int Order;

    public List<SubProcessSO> SubProcesses;
    public List<RewardSO> Rewards;

    /// <summary>
    /// Chuyển MainProcessSO → MainProcess
    /// </summary>
  
    public MainProcess.ProcessStatus Status = MainProcess.ProcessStatus.Locked;

    // Chuyển đổi MainProcessSO sang MainProcess
    public MainProcess ToMainProcess()
    {
        var mainProcess = new MainProcess
        {
            Id          = Id,
            Type        = Type,
            Name        = Name,
            Description = Description,
            Order       = Order,
            SubProcesses= SubProcesses.ConvertAll(so => so.ToSubProcess()),
            Rewards     = Rewards.ConvertAll(so => so.ToReward()),
            Status      = Status
        };
        //Debug.Log($"[MainProcessSO] Converted MainProcess '{Id}': {mainProcess.SubProcesses.Count} SubProcesses, {mainProcess.Rewards.Count} Rewards");
        return mainProcess;
    }
}

// ScriptableObject cho SubProcess
[System.Serializable]
public class SubProcessSO
{
    public string Id;

    public MainProcess.ProcessType Type;

    public string Name;
    public string Description;
    public int Order;
    
    public MainProcess.TriggerType Trigger;
    
    public List<ConditionSO> Conditions;
    public List<RewardSO> Rewards;
    
    public MainProcess.ProcessStatus Status = MainProcess.ProcessStatus.Locked;

    public SubProcess ToSubProcess()
    {
        var subProcess = new SubProcess
        {
            Id         = Id,
            Type       = Type,
            Name       = Name,
            Description= Description,
            Order      = Order,
            Trigger    = Trigger,
            Conditions = Conditions.ConvertAll(so => so.ToCondition()),
            Rewards    = Rewards.ConvertAll(so => so.ToReward()),
            Status     = Status
        };
        // Debug.Log($"[SubProcessSO] Converted SubProcess '{Id}': {subProcess.Conditions.Count} Conditions, {subProcess.Rewards.Count} Rewards");
        return subProcess;
    }
}

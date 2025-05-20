using UnityEngine;
using System.Collections.Generic;

// ScriptableObject tổng hợp chứa tất cả MainProcess
[CreateAssetMenu(fileName = "ProgressionData", menuName = "Progression/ProgressionData")]
public class ProgressionDataSO : ScriptableObject
{
    public List<MainProcessSO> MainProcesses;

    public GameProgression ToGameProgression()
    {
        var progression = new GameProgression
        {
            MainProcesses = MainProcesses.ConvertAll(so => so.ToMainProcess())
        };
        Debug.Log($"Converted ProgressionDataSO: {progression.MainProcesses.Count} MainProcesses");
        return progression;
    }
}

// ScriptableObject cho MainProcess
[System.Serializable]
public class MainProcessSO
{
    public string Id;
    public string Type;
    public string Name;
    public string Description;
    public int Order;
    public List<SubProcessSO> SubProcesses;
    public List<RewardSO> Rewards;

    public MainProcess ToMainProcess()
    {
        var mainProcess = new MainProcess
        {
            Id = Id,
            Type = Type,
            Name = Name,
            Description = Description,
            Order = Order,
            SubProcesses = SubProcesses.ConvertAll(so => so.ToSubProcess()),
            Rewards = Rewards.ConvertAll(so => so.ToReward()),
            Status = "Locked"
        };
        Debug.Log($"Converted MainProcess {Id}: {SubProcesses.Count} SubProcesses, {Rewards.Count} Rewards");
        return mainProcess;
    }
}

// ScriptableObject cho SubProcess
[System.Serializable]
public class SubProcessSO
{
    public string Id;
    public string Type;
    public string Name;
    public string Description;
    public int Order;
    public List<ConditionSO> Conditions;
    public List<RewardSO> Rewards;

    public SubProcess ToSubProcess()
    {
        var subProcess = new SubProcess
        {
            Id = Id,
            Type = Type,
            Name = Name,
            Description = Description,
            Order = Order,
            Conditions = Conditions.ConvertAll(so => so.ToCondition()),
            Rewards = Rewards.ConvertAll(so => so.ToReward()),
            Status = "Locked"
        };
        Debug.Log($"Converted SubProcess {Id}: {Conditions.Count} Conditions, {Rewards.Count} Rewards");
        return subProcess;
    }
}


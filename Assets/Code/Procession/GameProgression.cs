using System;
using System.Collections.Generic;
using Script.Procession.Conditions;
using Script.Procession.Reward.Base;
using UnityEngine;

/// <summary>
/// Lớp chính chứa toàn bộ dữ liệu tiến trình (Game Progression)
/// </summary>
[Serializable]
public class GameProgression
{
    // Danh sách các tiến trình chính
    public List<MainProcess> MainProcesses;
}

/// <summary>
/// Lớp cho tiến trình chính.
/// Định nghĩa các tiến trình con và phần thưởng cho tiến trình chính.
/// </summary>
[Serializable]
public class MainProcess
{
    //Enum
    public enum ProcessType { Chapter, Quest, Dialogue, Cutscene, Puzzle }
    public enum ProcessStatus { Locked, InProgress, Completed }
    public enum TriggerType { Manual, Automatic }
    
    public string Id;
    

    // Loại tiến trình (gọi ra dropdown trong Inspector nếu SO sử dụng enum trực tiếp)
    public ProcessType Type;

    // Trạng thái hiện tại (mặc định là Locked)
    public ProcessStatus Status = ProcessStatus.Locked;

    public string Name;
    public string Description;

    // Thứ tự xuất hiện (nhỏ → lớn)
    public int Order;

    // Danh sách tiến trình con (SubProcesses)
    public List<SubProcess> SubProcesses;

    // Danh sách phần thưởng khi MainProcess hoàn thành
    public List<Reward> Rewards;
}

/// <summary>
/// Lớp cho tiến trình con (SubProcess).
/// Định nghĩa các điều kiện thực hiện và phần thưởng cho tiến trình con.
/// </summary>
[Serializable]
public class SubProcess
{
    public string Id;

    // Sử dụng cùng enum ProcessType từ MainProcess
    public MainProcess.ProcessType Type;

    // Trạng thái hiện tại (mặc định là Locked)
    public MainProcess.ProcessStatus Status = MainProcess.ProcessStatus.Locked;

    public string Name;
    public string Description;
    
    public MainProcess.TriggerType Trigger;
    
    // Thứ tự trong MainProcess
    public int Order;

    // Danh sách điều kiện (Condition) để SubProcess được coi là hoàn thành
    public List<Condition> Conditions;

    // Danh sách phần thưởng khi SubProcess hoàn thành
    public List<Reward> Rewards;
}
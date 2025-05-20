using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lớp chính chứa toàn bộ dữ liệu tiến trình
/// </summary>
[Serializable]
public class GameProgression
{
    public List<MainProcess> MainProcesses;
}

/// <summary>
/// Lớp cho tiến trình chính.
/// định nghĩa các tiến trình con và phần thưởng cho tiến trình chính.
/// </summary>
[Serializable]
public class MainProcess
{
    public string Id;
    public string Type;                     // Loại tiến trình chính (Chapter, Campaign,...)
    public string Name;
    public string Description;
    public int Order;                       // Thứ tự
    public List<SubProcess> SubProcesses;   // Danh sách tiến trình con
    public List<Reward> Rewards;
    public string Status;                   // Trạng thái: Locked, InProgress, Completed
}

/// <summary>
/// Lớp cho tiến trình Con.
/// định nghĩa các điều kiện thực hiện và phần thưởng cho tiến trình con.
/// </summary>
[Serializable]
public class SubProcess
{
    public string Id;
    public string Type;                     // Loại tiến trình con (Quest, Task,...)
    public string Name;
    public string Description;
    public int Order;
    public List<Condition> Conditions;      // Danh sách điều kiện thực hiện
    public List<Reward> Rewards;
    public string Status;
}



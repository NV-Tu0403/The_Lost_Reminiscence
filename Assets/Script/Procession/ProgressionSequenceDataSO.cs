using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ProgressionSequenceData", menuName = "Game/Progression Sequence Data")]
public class ProgressionSequenceDataSO : ScriptableObject
{

    public List<string> MainProcessOrder; // Danh sách ID của các tiến trình chính theo thứ tự
}

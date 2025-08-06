using System;
using UnityEngine;

[Serializable]
public class DialogueConfigData
{
    public string description;
    public string[] dialogues;
}

[CreateAssetMenu(fileName = "DialogueConfig", menuName = "Configs/DialogueConfig", order = 1)]
public class DialogueConfig : ScriptableObject
{
    public DialogueConfigData[] dialogueSets;

    public DialogueConfig(DialogueConfigData[] dialogueSets)
    {
        this.dialogueSets = dialogueSets;
    }
}
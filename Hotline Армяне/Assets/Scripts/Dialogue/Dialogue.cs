using UnityEngine;

[System.Serializable]
public class Dialogue
{
    public string dialogueID;
    public DialogueLine[] lines;
}

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(3, 10)] public string text;
    public bool triggerNextObjectiveAfterThisLine = false;
}

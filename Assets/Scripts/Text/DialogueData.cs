using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    public string speakerName;
    public Sprite speakerAvatar;
    [TextArea(2, 5)]
    public string text;
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "404Entries/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public List<DialogueLine> lines = new List<DialogueLine>();
}

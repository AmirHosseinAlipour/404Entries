using System;
using System.Collections.Generic;
using UnityEngine;

public class Amozesh : MonoBehaviour
{
    [System.Serializable]
    public class DialoguePhase
    {
        public List<string> dialogues; 
    }

    public  List<DialoguePhase> dialoguePhases;
    public int currentPhase = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<DialoguePhase> getDialoguePhases()
    {
        return dialoguePhases;
    }

    public int getCurrentPhase()
    {
        return TaskManager.Instance.GetCurrentTaskIndex();
    }
}

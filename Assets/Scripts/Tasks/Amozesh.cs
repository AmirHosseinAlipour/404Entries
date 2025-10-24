using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Amozesh : MonoBehaviour
{
    [System.Serializable]
    public class DialoguePhase
    {
        public List<string> dialogues;
    }

    [Header("Dialogue Data")]
    public  List<DialoguePhase> dialoguePhases;

    [Header("Dialogue UI")]
    public List<Text> listOfTexts;
    public int listLength;
    public GameObject firstDialogue;

    private void Start()
    {
        for (int i = 0; i < listLength; i++)
        {
            listOfTexts[i].text = 
                dialoguePhases[GameManager.Instance.getCurrentTaskIndex()].dialogues[i];
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            firstDialogue.SetActive(true);
        }
    }
}

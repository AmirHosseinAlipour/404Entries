using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public List<TextMeshProUGUI> listOfTexts;
    public int listLength;
    public RectTransform firstDialogue;

    private int _lastCurrentIndex;
    private int currentIndex;
    public TaskUI taskUI;

    private void Start()
    {
        SetDialogues();
        
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnCurrentIndexChange += ChangeCurrentIndex;
        }
    }

    private void SetDialogues()
    {
        for (int i = 0; i < listLength; i++)
        {
            string text = dialoguePhases[currentIndex / 2].dialogues[i];
            listOfTexts[i].transform.parent.parent.gameObject.SetActive(true);
            listOfTexts[i].text = text;
            listOfTexts[i].GetComponent<NewFarsiTypewriter>().SetText(text);
            listOfTexts[i].transform.parent.parent.gameObject.SetActive(false);
        }
    }
    
    public void ChangeCurrentIndex()
    {
        _lastCurrentIndex = currentIndex;
        currentIndex = TaskManager.Instance.GetCurrentTaskIndex();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (_lastCurrentIndex != currentIndex)
            {
                SetDialogues();
            }

            UIAnimationManager.Instance.ShowDialogueWindow(
                firstDialogue, 0.5f, listOfTexts[0].GetComponent<NewFarsiTypewriter>());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            taskUI.UpdateUI();
        }
    }
}

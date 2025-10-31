using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class Safaei : BaseProfessors
{
        public TMP_Text dialogueText;
        public Button[] optionButtons;
        public TypeWriterEffect typewriter;
        public RectTransform ButtonCanvas;
        [TextArea(3, 10)] public string[] phaseTexts;
        public string[,] phaseOptions = new string[5, 3]
        {
            { "Option 1-1", "Option 1-2", "Option 1-3" },
            { "Option 2-1", "Option 2-2", "Option 2-3" },
            { "Option 3-1", "Option 3-2", "Option 3-3" }, 
            { "Option 4-1", "Option 4-2", "Option 4-3" }, 
            { "Option 5-1", "Option 5-2", "Option 5-3" }
        };

        public int[] correctOptionIndex;

        private int currentPhase = 0;
        private bool isTyping = false;
        private float baseSpeed = 0.05f;
        private float currentSpeed;

        void Start()
        {
            base.Start();
            currentSpeed = baseSpeed;
            HideAllOptions();
        }

        public void StartGame()
        {
            StartCoroutine(StartPhase());
        }

        IEnumerator StartPhase()
        {
            if (currentPhase >= phaseTexts.Length)
            {
                TaskManager.Instance.CompleteTask(TaskOrderNumber);
        
                HideAllOptions();
                RectTransform parent = StartPanel.transform.parent.GetComponent<RectTransform>();
                parent.gameObject.SetActive(false);
                UIAnimationManager.Instance.HideWindow(parent, 0.5f);
                UIAnimationManager.Instance.ShowDialogueWindow(firstDialogue, 0.5f, firstDialogueFtw);

                yield break;
            }

            HideAllOptions();
            isTyping = true;
            yield return typewriter.PlayText(dialogueText, phaseTexts[currentPhase], currentSpeed);
            isTyping = false;
            ShowOptions();
        }

        void HideAllOptions()
        {
           UIAnimationManager.Instance.HideWindow(ButtonCanvas, 0.5f);
        }

        void ShowOptions()
        {
            UIAnimationManager.Instance.ShowWindow(ButtonCanvas, 0.5f);
            
            for (int i = 0; i < optionButtons.Length; i++)
            {
                int index = i;
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => OnOptionSelected(index));
                optionButtons[i].GetComponentInChildren<TMP_Text>().text = phaseOptions[currentPhase, i];
            }
        }

        void OnOptionSelected(int index)
        {
            HideAllOptions();
            bool isCorrect = (index == correctOptionIndex[currentPhase]);
            if (!isCorrect)
            {
                currentSpeed /= 2f;
            }

            currentPhase++;
            StartCoroutine(StartPhase());
        }
    }
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaperManager : MonoBehaviour
{
    public static PaperManager Instance;
    
    [Header("Mini Game")]
    public RectTransform task;
    public FailOrPass prof;

    [Header("Papers")]
    public List<GameObject> listOfPapers;
    public float rotationDuration = 0.25f;
    public FarsiTypewriter counterFtw;

    private List<GameObject> _failedPapers;
    private List<GameObject> _passedPapers;
    
    [Header("Confirmation Dialogue")]
    public RectTransform dialogueBox;
    public Text[] texts;

    private int _currentItemIndex;
    
    private DraggableItem _currentItem;
    private DropZone _currentZone;

    private int _textLength;

    private int _papersLength;
    private int _counter;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _textLength = texts.Length;
        _papersLength = listOfPapers.Count;
        _counter = _papersLength;
        
        _failedPapers = new List<GameObject>();
        _passedPapers = new List<GameObject>();
    }

    private void Start()
    {
        counterFtw.SetText(_papersLength + " / " + _counter);
        counterFtw.StartTyping();
        
        foreach (GameObject paper in listOfPapers)
        {
            paper.SetActive(false);
        }

        if (listOfPapers.Count > 0 && listOfPapers[0] != null)
        {
            listOfPapers[0].SetActive(true);
            listOfPapers[0].transform.localRotation = Quaternion.Euler(0, 0, -10);
        }
        if (listOfPapers.Count > 1 && listOfPapers[1] != null)
        {
            listOfPapers[1].SetActive(true);
            listOfPapers[1].transform.localRotation = Quaternion.Euler(0, 0, -5);
        }
        if (listOfPapers.Count > 2 && listOfPapers[2] != null)
        {
            listOfPapers[2].SetActive(true);
            listOfPapers[2].transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        
        _currentItemIndex = 0;
    }

    public void ShowConfirmation(DraggableItem item, DropZone zone)
    {
        _currentItem = item;
        _currentZone = zone;
        
        UIAnimationManager.Instance.ShowWindow(dialogueBox, 0.5f);
        StartCoroutine(TextSequence());
    }
    
    private IEnumerator TextSequence()
    {
        for (int i = 0; i < _textLength; i++)
        {
            texts[i].text = "";
        }
        
        for (int i = 0; i < _textLength; i++)
        {
            FarsiTypewriter text = texts[i].gameObject.GetComponent<FarsiTypewriter>();
            int len = text.len;
            text.StartTyping();
            yield return new WaitForSeconds(text.typeCharTime * len);
        }
    }

    public void OnYesClicked()
    {
        if (_currentItem != null)
        {
            _currentItem.CompleteDrop();
            _counter--;
            
            UpdateDropZone(_currentItem);
            StartCoroutine(UpdatePaperStackRoutine(rotationDuration));
            
            _currentItem = null;
            _currentZone = null;

            UIAnimationManager.Instance.HideWindow(dialogueBox, 0.5f);
        }
    }

    private void UpdateDropZone(DraggableItem currentItem)
    {
        List<GameObject> targetList = _currentZone.isPassSlot ? _passedPapers : _failedPapers;

        if (targetList.Count > 0)
        {
            GameObject previousPaper = targetList[^1];
            if (previousPaper != null)
            {
                previousPaper.SetActive(false);
            }
        }
        
        targetList.Add(currentItem.gameObject);
    }

    private IEnumerator UpdatePaperStackRoutine(float duration)
    {
        Transform paperToRotate1 = null;
        Transform paperToRotate2 = null;

        if (_currentItemIndex + 1 < listOfPapers.Count && listOfPapers[_currentItemIndex + 1] != null)
        {
            paperToRotate1 = listOfPapers[_currentItemIndex + 1].transform;
        }
        if (_currentItemIndex + 2 < listOfPapers.Count && listOfPapers[_currentItemIndex + 2] != null)
        {
            paperToRotate2 = listOfPapers[_currentItemIndex + 2].transform;
        }

        Quaternion startRot1 = (paperToRotate1 != null) ? paperToRotate1.localRotation : Quaternion.identity;
        Quaternion targetRot1 = startRot1 * Quaternion.Euler(0, 0, -5);

        Quaternion startRot2 = (paperToRotate2 != null) ? paperToRotate2.localRotation : Quaternion.identity;
        Quaternion targetRot2 = startRot2 * Quaternion.Euler(0, 0, -5);

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration); 

            if (paperToRotate1 != null)
            {
                paperToRotate1.localRotation = Quaternion.Lerp(startRot1, targetRot1, t);
            }
            if (paperToRotate2 != null)
            {
                paperToRotate2.localRotation = Quaternion.Lerp(startRot2, targetRot2, t);
            }

            yield return null;
        }

        if (paperToRotate1 != null)
        {
            paperToRotate1.localRotation = targetRot1;
        }
        if (paperToRotate2 != null)
        {
            paperToRotate2.localRotation = targetRot2;
        }

        if (_currentItemIndex + 3 < listOfPapers.Count && listOfPapers[_currentItemIndex + 3] != null)
        {
            listOfPapers[_currentItemIndex + 3].SetActive(true);
            listOfPapers[_currentItemIndex + 3].transform.localRotation = Quaternion.Euler(0, 0, 0);
        }

        _currentItemIndex++;
        
        counterFtw.SetText(_papersLength + " / " + _counter);
        counterFtw.StartTyping();
        
        // End logic
        if (_counter == 0)
        {
            TaskManager.Instance.CompleteTask(prof.TaskOrderNumber);
            UIAnimationManager.Instance.HideWindow(task, 0.5f);
            if (prof.firstDialogue != null && prof.firstDialogueFtw != null)
            {
                UIAnimationManager.Instance.ShowDialogueWindow(
                    prof.firstDialogue, 0.5f, prof.firstDialogueFtw
                );
            }
        }
    }

    public void OnNoClicked()
    {
        if (_currentItem != null)
        {
            _currentItem.CancelDrop();
        }

        _currentItem = null;
        _currentZone = null;
        
        UIAnimationManager.Instance.HideWindow(dialogueBox, 0.5f);
    }
}
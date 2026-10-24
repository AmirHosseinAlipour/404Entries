using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BaseTasks : MonoBehaviour
{
    [Header("Base Fields")]
    public RectTransform StartPanel;
    public Text[] listOfTexts;
    
    private const int InitialDialogueCount = 3;

    protected PlayerController _player;
    
    private Button _allTasksBackButton;
    private Button _currentTaskButton;
    
    private void Awake()
    {
        FindReferences();
    }

    private void FindReferences()
    {
        if (_player == null)
        {
            var pObj = GameObject.FindWithTag("Player");
            if (pObj != null) _player = pObj.GetComponent<PlayerController>();
        }

        if (_allTasksBackButton == null)
        {
            var backObj = GameObject.FindWithTag("AllTasksBackButton");
            if (backObj != null) _allTasksBackButton = backObj.GetComponent<Button>();
        }

        if (_currentTaskButton == null)
        {
            var curObj = GameObject.FindWithTag("CurrentTaskButton");
            if (curObj != null) _currentTaskButton = curObj.GetComponent<Button>();
        }
    }
    
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            FindReferences();
            PlayerUIModeHelper.PlayerEnterUIMode(_player);
            PlayerUIModeHelper.DisableTasksButton(_allTasksBackButton, _currentTaskButton);
                
            if (StartPanel != null)
            {
                UIAnimationManager.Instance.ShowWindow(StartPanel, 0.5f);
            }
                
            if (listOfTexts != null)
            {
                for (int i = 0; i < Mathf.Min(3, listOfTexts.Length); i++)
                {
                    if (listOfTexts[i] != null)
                    {
                        listOfTexts[i].text = "";
                    }
                }
            }
            
            StartCoroutine(TextSequence());
        }
    }
    
    private IEnumerator TextSequence()
    {
        for (int i = 0; i < InitialDialogueCount; i++)
        {
            if (listOfTexts != null && i < listOfTexts.Length && listOfTexts[i] != null)
            {
                FarsiTypewriter text = listOfTexts[i].gameObject.GetComponent<FarsiTypewriter>();
                if (text != null)
                {
                    int len = text.len;
                    text.StartTyping();
                    yield return new WaitForSeconds(text.typeCharTime * len);
                }
            }
        }
    }
}

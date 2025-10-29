using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BaseTasks : MonoBehaviour
{
    [Header("Base Fields")]
    // Accept challenge message
    public RectTransform StartPanel;
    public Text[] listOfTexts;
    
    private const int InitialDialogueCount = 3;

    protected PlayerController _player;
    
    [Header("Task UI")]
    public Button allTasksBackButton;
    public Button currentTaskButton;
    
    private void Awake()
    {
        _player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }
    
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerUIModeHelper.PlayerEnterUIMode(_player);
            PlayerUIModeHelper.DisableTasksButton(allTasksBackButton, currentTaskButton);
                
            UIAnimationManager.Instance.ShowWindow(StartPanel, 0.5f);
                
            for (int i = 0; i < 3; i++)
            {
                // Reset the text so each time the text would be written in type writer effect!
                listOfTexts[i].text = "";
            }
            
            StartCoroutine(TextSequence());
        }
    }
    
    // To show the accept challenge message in a proper type writer order!
    private IEnumerator TextSequence()
    {
        // First of all we write the first 3 text which is the accept message + yes/no option
        for (int i = 0; i < InitialDialogueCount; i++)
        {
            FarsiTypewriter text = listOfTexts[i].gameObject.GetComponent<FarsiTypewriter>();
            int len = text.len;
            text.StartTyping();
            yield return new WaitForSeconds(text.typeCharTime * len);
        }
    }
}

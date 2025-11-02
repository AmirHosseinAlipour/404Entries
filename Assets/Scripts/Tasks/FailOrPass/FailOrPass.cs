using UnityEngine;
using UnityEngine.UI;

public class FailOrPass : BaseProfessors
{
    [Header("Mini Game")]
    public RectTransform task;
    public FarsiTypewriter letter;
    public Button firstLetter;
    private MusicChange musicChange;
    
    private void Start()
    {
        if (letter != null)
        {
            letter.OnTypingFinished += EnableButton;
        }

        musicChange = GetComponent<MusicChange>();
    }

    public void StartGame()
    {
        UIAnimationManager.Instance.ShowWindow(task, 0.5f);

        if (firstLetter != null)
        {
            firstLetter.interactable = false;
        }
        musicChange.ToggleMusic();
        musicChange.PlayThemeMusic();
        letter.StartTyping();
    }

    private void EnableButton()
    {
        if (firstLetter != null)
        {
            firstLetter.interactable = true;
        }
    }

    private void OnDestroy()
    {
        if (letter != null)
        {
            letter.OnTypingFinished -= EnableButton;
        }
    }
}
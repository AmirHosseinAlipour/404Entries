using Unity.Cinemachine;
using UnityEngine;

public class AliAkbari : BaseProfessors
{
    public override ProfessorUIType UIType => ProfessorUIType.InWorldGame;

    [Header("Mini Game")] 
    public GameObject pongGame;
    public PongManager fadeActivator;
    private MusicChange musicChange;

    protected override void Start()
    {
        base.Start();
        musicChange = GetComponent<MusicChange>();
        HideMinigameUI();
        ApplyModernVisualStyles();
    }

    public override void HideMinigameUI()
    {
        base.HideMinigameUI();
        if (pongGame != null) pongGame.SetActive(false);
    }

    public override void AcceptChallenge()
    {
        StartGame();
    }

    public void StartGame()
    {
        StartMinigame();
        if (fadeActivator != null) fadeActivator.InitialSettings();
        if (pongGame != null) pongGame.SetActive(true);
        if (fadeActivator != null) fadeActivator.FadeInAndActivate();
        if (musicChange != null)
        {
            musicChange.ToggleMusic();
            musicChange.PlayThemeMusic();
        }
        ApplyModernVisualStyles();
    }
}

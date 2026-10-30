using System.Collections;
using UnityEngine;

public class TeacherController : BaseProfessors
{
    public override ProfessorUIType UIType => ProfessorUIType.InWorldStealth;

    public Animator animator;
    public bool isFacingPlayer = false;
    private bool flag = false; 
    [Header("Random Rotation Settings")]
    public float minWait = 1.5f;
    public float maxWait = 4f;

    private bool _isActive = false; 
    private Coroutine lookRoutine;
    private SoundPlayer soundPlayer;

    protected override void Awake()
    {
        base.Awake();
        soundPlayer = GetComponent<SoundPlayer>();
    }

    protected override void Start()
    {
        base.Start();
        if (GetComponent<AsgharRoomCamera>() == null)
        {
            gameObject.AddComponent<AsgharRoomCamera>();
        }
        if (animator == null) animator = GetComponent<Animator>();
        if (animator != null) animator.Play("FacingBack");
        HideMinigameUI();
        ApplyModernVisualStyles();
    }

    public override void HideMinigameUI()
    {
        base.HideMinigameUI();
        _isActive = false;
        if (lookRoutine != null)
        {
            StopCoroutine(lookRoutine);
            lookRoutine = null;
        }
        if (animator != null) animator.Play("FacingBack");
    }

    public override void AcceptChallenge()
    {
        StartLooking();
    }

    public void StartLooking()
    {
        StartMinigame();
        PlayerUIModeHelper.PlayerExitUIMode(_player);
        if (AsgharRoomCamera.Instance != null)
        {
            AsgharRoomCamera.Instance.EnterRoom();
        }
        if (_isActive) return;
        _isActive = true;
        lookRoutine = StartCoroutine(RandomLookRoutine());
    }

    private IEnumerator RandomLookRoutine()
    {
        while (_isActive)
        {
            yield return new WaitForSeconds(Random.Range(minWait, maxWait));
        
            if (isFacingPlayer)
            {
                if (animator != null) animator.SetTrigger("FacingBack");
                isFacingPlayer = false; 
            }
            else
            {
                if (animator != null) animator.SetTrigger("FacingFront");
            }
        }
    }

    public void PlayGreenLightSound()
    {
        if (flag == true)
        {
            if (soundPlayer != null) soundPlayer.Play("GreenLight");
        }
        else
        {
            flag = true;
        }
    }
    
    public void OnFacingFrontComplete()
    {
        isFacingPlayer = true;
        if (soundPlayer != null) soundPlayer.Play("RedLight");
    }

    public void StopLooking()
    {
        _isActive = false;
        if (lookRoutine != null)
            StopCoroutine(lookRoutine);
        if (animator != null)
            animator.Play("FacingBack");
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
    }
}
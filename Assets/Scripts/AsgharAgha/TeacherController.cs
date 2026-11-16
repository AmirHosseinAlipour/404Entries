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
    private float _lastGreenSoundTime = -10f;
    private float _lastRedSoundTime = -10f;

    protected override void Awake()
    {
        base.Awake();
        soundPlayer = GetComponent<SoundPlayer>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    protected override void Start()
    {
        base.Start();
        if (GetComponent<AsgharRoomCamera>() == null)
        {
            gameObject.AddComponent<AsgharRoomCamera>();
        }
        if (animator == null) animator = GetComponent<Animator>();
        if (soundPlayer == null) soundPlayer = GetComponent<SoundPlayer>();

        // Stand facing the chalkboard initially
        isFacingPlayer = false;
        if (animator != null)
        {
            animator.Play("BackIdle");
        }

        HideMinigameUI();
        ApplyModernVisualStyles();
    }

    public override void HideMinigameUI()
    {
        base.HideMinigameUI();
        _isActive = false;
        flag = false;
        isFacingPlayer = false;
        if (lookRoutine != null)
        {
            StopCoroutine(lookRoutine);
            lookRoutine = null;
        }
        if (soundPlayer != null)
        {
            AudioSource src = soundPlayer.GetComponent<AudioSource>();
            if (src != null) src.Stop();
        }
        if (animator != null)
        {
            animator.ResetTrigger("FacingFront");
            animator.ResetTrigger("FacingBack");
            animator.Play("BackIdle");
        }
    }

    public override void AcceptChallenge()
    {
        StartLooking();
    }

    public void StartLooking()
    {
        StartMinigame();

        if (_player == null)
        {
            var pObj = GameObject.FindWithTag("Player");
            if (pObj != null) _player = pObj.GetComponent<PlayerController>();
        }
        PlayerUIModeHelper.PlayerExitUIMode(_player);
        if (_player != null)
        {
            _player.isUIActive = false;
        }

        if (PlatformHelper.IsMobile())
        {
            var joy = GameObject.FindWithTag("JoyStick");
            if (joy != null) UIUtils.SetAlpha(joy, 1f);
        }

        if (AsgharRoomCamera.Instance != null)
        {
            AsgharRoomCamera.Instance.EnterRoom();
        }

        if (_isActive) return;
        _isActive = true;

        if (animator == null) animator = GetComponent<Animator>();
        if (soundPlayer == null) soundPlayer = GetComponent<SoundPlayer>();

        // Face the chalkboard so player can sneak into the room
        isFacingPlayer = false;
        if (animator != null)
        {
            animator.ResetTrigger("FacingFront");
            animator.ResetTrigger("FacingBack");
            animator.Play("BackIdle");
        }

        // Play initial Green Light chant
        PlayGreenLightSound();

        lookRoutine = StartCoroutine(RandomLookRoutine());
    }

    private IEnumerator RandomLookRoutine()
    {
        while (_isActive)
        {
            yield return new WaitForSeconds(Random.Range(minWait, maxWait));
            if (!_isActive) yield break;

            if (isFacingPlayer)
            {
                TurnToBoard();
            }
            else
            {
                TurnToPlayer();
            }
        }
    }

    public void TurnToPlayer()
    {
        if (!_isActive) return;
        if (animator != null)
        {
            animator.ResetTrigger("FacingBack");
            animator.SetTrigger("FacingFront");
            animator.Play("FacingFront", 0, 0f);
        }
        StartCoroutine(DelayedFacingPlayer());
    }

    private IEnumerator DelayedFacingPlayer()
    {
        yield return new WaitForSeconds(0.085f);
        if (_isActive)
        {
            OnFacingFrontComplete();
        }
    }

    public void TurnToBoard()
    {
        if (!_isActive) return;
        // Player is immediately safe to move when teacher starts turning back to board
        isFacingPlayer = false;
        if (animator != null)
        {
            animator.ResetTrigger("FacingFront");
            animator.SetTrigger("FacingBack");
            animator.Play("FacingBack", 0, 0f);
        }
        StartCoroutine(DelayedFacingBoard());
    }

    private IEnumerator DelayedFacingBoard()
    {
        yield return new WaitForSeconds(0.085f);
        if (_isActive)
        {
            PlayGreenLightSound();
        }
    }

    public void PlayGreenLightSound()
    {
        if (!_isActive) return;
        if (Time.time - _lastGreenSoundTime < 0.4f) return;
        _lastGreenSoundTime = Time.time;

        if (soundPlayer == null) soundPlayer = GetComponent<SoundPlayer>();
        if (soundPlayer != null)
        {
            soundPlayer.Play("GreenLight");
        }
    }
    
    public void OnFacingFrontComplete()
    {
        if (!_isActive) return;
        isFacingPlayer = true;
        if (Time.time - _lastRedSoundTime < 0.4f) return;
        _lastRedSoundTime = Time.time;

        if (soundPlayer == null) soundPlayer = GetComponent<SoundPlayer>();
        if (soundPlayer != null)
        {
            soundPlayer.Play("RedLight");
        }
    }

    public void StopLooking()
    {
        _isActive = false;
        flag = false;
        isFacingPlayer = false;
        if (lookRoutine != null)
        {
            StopCoroutine(lookRoutine);
            lookRoutine = null;
        }
        if (soundPlayer != null)
        {
            AudioSource src = soundPlayer.GetComponent<AudioSource>();
            if (src != null) src.Stop();
        }
        if (animator != null)
        {
            animator.ResetTrigger("FacingFront");
            animator.ResetTrigger("FacingBack");
            animator.Play("BackIdle");
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
    }
}

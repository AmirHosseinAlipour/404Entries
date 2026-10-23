using System.Collections;
using UnityEngine;

public class TeacherController : BaseProfessors
{
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

    private void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (animator != null) animator.Play("FacingBack");
    }

    public void StartLooking()
    {
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
                // Player is safe immediately
                if (animator != null) animator.SetTrigger("FacingBack");
                isFacingPlayer = false; 
            }
            else
            {
                // Just trigger the animation.
                // The Animation Event will call OnFacingFrontComplete() when it's done.
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
        //ignore the base method!
    }
}
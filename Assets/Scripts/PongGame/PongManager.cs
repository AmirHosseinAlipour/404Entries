using System;
using Unity.Cinemachine;
using UnityEngine;

public class PongManager : MonoBehaviour
{
    [Header("Fade Settings")]
    public GameObject targetObject;
    public float fadeSpeed = 2f;
    public GameObject PongGameObject;
    public CinemachineCamera cam;
    public SpriteRenderer blackBackground;
    private MusicChange musicChange;

    public void InitialSettings()
    {
        cam.Priority = 20;
        ScaleSpriteToCamera();
    }

    [Header("Professor")] 
    public AliAkbari prof;

    private Renderer[] renderers;
    private bool isFadingIn = false;
    private bool isFadingOut = false;

    private GameObject _currentTaskButton;

    void Awake()
    {
        if (targetObject != null)
        {
            // همه‌ی Rendererهای فرزند رو می‌گیریم (برای Mesh یا Sprite)
            renderers = targetObject.GetComponentsInChildren<Renderer>();
            targetObject.SetActive(false);
        }

        _currentTaskButton = GameObject.FindWithTag("CurrentTaskButton");
        musicChange = GetComponent<MusicChange>();
    }

    void Update()
    {
        if (isFadingIn)
        {
            bool done = FadeTo(1f);
            if (done)
                isFadingIn = false;
        }
        else if (isFadingOut)
        {
            bool done = FadeTo(0f);
            if (done)
            {
                isFadingOut = false;
                targetObject.SetActive(false);
                PongGameObject.SetActive(false);
                UIUtils.SetAlpha(_currentTaskButton.gameObject, 1f);
                cam.Priority = 0;
                blackBackground.sortingOrder = -10;
                TaskManager.Instance.CompleteTask(prof.TaskOrderNumber);
                UIAnimationManager.Instance.ShowDialogueWindow(prof.firstWinDialogue, 0.5f, prof.firstWinDialogueFtw);
                musicChange.ToggleMusic();
                musicChange.PlayMainMusic();
                
            }
        }
    }

    public void FadeInAndActivate()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(true);
            SetAlpha(0f);
            isFadingIn = true;
        }
    }

    public void FadeOutAndDeactivate()
    {
        if (targetObject != null)
        {
            isFadingOut = true;
        }
    }

    private bool FadeTo(float targetAlpha)
    {
        bool allDone = true;
        foreach (var rend in renderers)
        {
            Color c = rend.material.color;
            float newAlpha = Mathf.MoveTowards(c.a, targetAlpha, fadeSpeed * Time.deltaTime);
            if (!Mathf.Approximately(newAlpha, targetAlpha))
                allDone = false;
            c.a = newAlpha;
            rend.material.color = c;
        }
        return allDone;
    }

    private void SetAlpha(float alpha)
    {
        foreach (var rend in renderers)
        {
            Color c = rend.material.color;
            c.a = alpha;
            rend.material.color = c;
        }
    }
    private void ScaleSpriteToCamera()
    {
        float cameraHeight = cam.Lens.OrthographicSize * 2f;
        float cameraWidth = cameraHeight * cam.Lens.Aspect;

        float spriteHeight = blackBackground.sprite.bounds.size.y;
        float spriteWidth = blackBackground.sprite.bounds.size.x;

        float scaleX = cameraWidth / spriteWidth;
        float scaleY = cameraHeight / spriteHeight;

        float finalScale = Mathf.Max(scaleX, scaleY);

        blackBackground.transform.localScale = new Vector3(finalScale, finalScale, 1f);
        blackBackground.sortingOrder = 10;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TaskManager : MonoBehaviour
{
    [Header("Ending")] 
    public ScreenFaderEnd screenFaderEnd;
    
    [Header("Task Data")]
    public string[] taskNames;
    public bool[] taskCompleted;

    public static TaskManager Instance;
    public Sprite Boy_image;
    public Sprite Girl_image;
    public Image Main_image;
    
    public Action OnCurrentIndexChange;
    
    [Header("Task Triggers")]
    public List<GameObject> taskTriggers;

    [Header("Testing & Mobile Controls")]
    [Tooltip("Tick this box to force the mobile virtual joystick to appear on PC so you can test it with the mouse.")]
    public bool forceShowJoystick = false;

    [Header("Language / دو زبانه")]
    [Tooltip("Choose game dialogue language (Persian / English). You can also press F3 at runtime!")]
    public GameLanguage gameLanguage = GameLanguage.Persian;

    private void Start()
    {
        if (forceShowJoystick)
        {
            PlatformHelper.SetForceShowJoystick(true);
        }
        LocalizationManager.SetLanguage(gameLanguage);
        SetImage();
    }

    private void Update()
    {
        if (LocalizationManager.CurrentLanguage != gameLanguage)
        {
            gameLanguage = LocalizationManager.CurrentLanguage;
        }
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        
        if (taskTriggers != null && taskTriggers.Count > 0 && taskTriggers[0] != null)
        {
            Target target = taskTriggers[0].GetComponent<Target>();
            if (target != null) target.EnableTarget();
        }
    }
    
    // For debugging
    private void OnValidate()
    {
        // Only invoke if we're in play mode
        if (Application.isPlaying)
        {
            PlatformHelper.SetForceShowJoystick(forceShowJoystick);
            LocalizationManager.SetLanguage(gameLanguage);
            OnCurrentIndexChange?.Invoke();
        }
    }
    public void CompleteTask(int index)
    {
        if (index >= 0 && index < taskCompleted.Length)
        {
            taskCompleted[index] = true;
            OnCurrentIndexChange?.Invoke();
            
            if (taskTriggers != null)
            {
                if (index < taskTriggers.Count && taskTriggers[index] != null)
                {
                    Target currentTarget = taskTriggers[index].GetComponent<Target>();
                    if (currentTarget != null) currentTarget.DisableTarget();
                }

                int nextIndex = GetCurrentTaskIndex();
                if (nextIndex >= 0 && nextIndex < taskTriggers.Count && taskTriggers[nextIndex] != null)
                {
                    Target nextTarget = taskTriggers[nextIndex].GetComponent<Target>();
                    if (nextTarget != null) nextTarget.EnableTarget();
                }
            }
        }
    }
    public int GetCurrentTaskIndex()
    {
        for (int i = 0; i < taskCompleted.Length; i++)
        {
            if (!taskCompleted[i])
                return i;
        }
        return -1; 
    }

    public void SetImage()
    {
        int index = (SelectionPlayer.instance != null) ? SelectionPlayer.instance.PlayerID : 0;
        if (Main_image != null)
        {
            if (index == 0)
            {
                if (Girl_image != null) Main_image.sprite = Girl_image;
            }
            else
            {
                if (Boy_image != null) Main_image.sprite = Boy_image;
            }
        }
    }

    public void HandleEnding()
    {
        if (screenFaderEnd != null)
        {
            screenFaderEnd.gameObject.SetActive(true);
            StartCoroutine(FadeAndEndRoutine());
        }
        else
        {
            SceneManager.LoadScene("Ending");
        }
    }

    private IEnumerator FadeAndEndRoutine()
    {
        screenFaderEnd.StartFade(1f, 1.5f);
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("Ending");
    }
}
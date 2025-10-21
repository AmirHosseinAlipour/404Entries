using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AsgharManager : MonoBehaviour
{
    public TeacherController teacher;
    public PlayerController player;
    public GameObject fadePanel; 
    public float fadeDuration = 1f;
    public GameObject RespawnPoint;
    private CanvasGroup canvasGroup;
   
    private void Awake()
    {
        canvasGroup = fadePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            Debug.LogError("CanvasGroup component not found on FadePanel!");
    }

    private void Update()
    {
        

        
        if (teacher.isFacingPlayer && player.IsMoving())
        {
            StartCoroutine(RestartGame());
        }
    }

    public void WinGame()
    {
        StartCoroutine(WinRoutine());
    }

    private IEnumerator RestartGame()
    {
        
        yield return StartCoroutine(FadeIn(fadeDuration));
        player.Respawn(RespawnPoint.transform.position);
        yield return StartCoroutine(FadeOut(fadeDuration));
        
    }

    private IEnumerator WinRoutine()
    {
        fadePanel.SetActive(true);
        
        yield return new WaitForSeconds(fadeDuration);
        Debug.Log("You escaped the class!");
    }
    public IEnumerator FadeIn(float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, time / duration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }
    public IEnumerator FadeOut(float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, time / duration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }

}
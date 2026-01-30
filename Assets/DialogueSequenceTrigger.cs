using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogueSequenceTrigger : MonoBehaviour
{
    [Header("Dialogue Windows (In Order)")]
    public List<RectTransform> dialogueWindows;
    public List<FarsiTypewriter> typewriters; // optional

    [Header("Animation Durations")]
    public float showDuration = 0.5f;
    public float hideDuration = 0.3f;

    [Header("After Finish")]
    public bool destroyAfterFinish = true;

    private int currentIndex = 0;
    private bool inConversation = false;
    private Coroutine _exitRoutine;

    private void Start()
    {
        // اول کار: همه خاموش باشن
        foreach (var dlg in dialogueWindows)
        {
            if (dlg != null)
                dlg.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // اگر داشتیم خروج انیمیشنی انجام می‌دادیم، کنسلش کن
        if (_exitRoutine != null)
        {
            StopCoroutine(_exitRoutine);
            _exitRoutine = null;
        }

        // اگر همین الان وسط مکالمه‌ایم، دوباره شروع نکن
        if (inConversation) return;

        inConversation = true;
        currentIndex = 0;
        ShowCurrent();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // خروج => با انیمیشن جمع کن و ریست کن
        if (_exitRoutine != null) StopCoroutine(_exitRoutine);
        _exitRoutine = StartCoroutine(ExitAndResetRoutine());
    }

    public void NextDialogue()
    {
        if (!inConversation) return;

        HideCurrentAnimated();
        currentIndex++;

        if (currentIndex >= dialogueWindows.Count)
        {
            EndConversation();
            return;
        }

        ShowCurrent();
    }

    // ---------- Show / Hide ----------
    private void ShowCurrent()
    {
        if (currentIndex < 0 || currentIndex >= dialogueWindows.Count) return;

        var window = dialogueWindows[currentIndex];
        if (window == null) return;

        // اگر تایپ‌رایتر برای همین index داریم
        if (typewriters != null &&
            currentIndex < typewriters.Count &&
            typewriters[currentIndex] != null)
        {
            UIAnimationManager.Instance.ShowDialogueWindow(window, showDuration, typewriters[currentIndex]);
        }
        else
        {
            UIAnimationManager.Instance.ShowWindow(window, showDuration);
        }
    }

    private void HideCurrentAnimated()
    {
        if (currentIndex < 0 || currentIndex >= dialogueWindows.Count) return;

        var window = dialogueWindows[currentIndex];
        if (window == null) return;

        UIAnimationManager.Instance.HideWindow(window, hideDuration);
    }

    private void HideAllAnimated()
    {
        foreach (var dlg in dialogueWindows)
        {
            if (dlg != null && dlg.gameObject.activeSelf)
                UIAnimationManager.Instance.HideWindow(dlg, hideDuration);
        }
    }

    // ---------- Exit / Reset ----------
    private IEnumerator ExitAndResetRoutine()
    {
        // اگه وسط مکالمه نیستیم، کاری نکن
        if (!inConversation) yield break;

        // همه پنجره‌های فعال با انیمیشن بسته بشن
        HideAllAnimated();

        // صبر کن انیمیشن تموم شه
        yield return new WaitForSeconds(hideDuration);

        // بعد از انیمیشن، برای اینکه همه چیز تمیز باشه خاموششون کن
        foreach (var dlg in dialogueWindows)
        {
            if (dlg != null)
                dlg.gameObject.SetActive(false);
        }

        // ریست مکالمه
        currentIndex = 0;
        inConversation = false;
        _exitRoutine = null;
    }

    private void EndConversation()
    {
        // مکالمه تموم شد
        HideAllAnimated();
        inConversation = false;

        if (destroyAfterFinish)
        {
            // بذار انیمیشن خروج تموم شه بعد دیلیت کنیم
            if (_exitRoutine != null) StopCoroutine(_exitRoutine);
            _exitRoutine = StartCoroutine(DestroyAfterHide());
        }
    }

    private IEnumerator DestroyAfterHide()
    {
        yield return new WaitForSeconds(hideDuration);

        foreach (var dlg in dialogueWindows)
        {
            if (dlg != null)
                Destroy(dlg.gameObject);
        }

        Destroy(gameObject);
    }
}

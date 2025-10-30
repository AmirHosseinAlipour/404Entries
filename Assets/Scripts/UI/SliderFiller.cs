using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SliderFiller : MonoBehaviour
{
    public Slider slider;      
    public float fillTime = 2f; 
    public SpaceInvadersManager spaceInvadersManager;
    public RectTransform Boot_panel;
    void Start()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        slider.value = 0; 
        StartCoroutine(FillSlider());
    }

    IEnumerator FillSlider()
    {
        float elapsed = 0f;

        while (elapsed < fillTime)
        {
            elapsed += Time.deltaTime;
            slider.value = Mathf.Clamp01(elapsed / fillTime);
            yield return null;
        }

        slider.value = 1f;
        UIAnimationManager.Instance.HideWindow(Boot_panel , 0.5f);
        spaceInvadersManager.PlayerEnteredTrigger();
        
    }
}
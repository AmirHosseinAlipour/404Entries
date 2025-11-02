using UnityEngine;

public class Pause : MonoBehaviour
{
   public RectTransform pausePanel;
   public void OnPause()
   {
      Time.timeScale = 0;
      UIAnimationManager.Instance.ShowWindow(pausePanel , 0.5f);
   }

   public void OnUnpause()
   {
      Time.timeScale = 1;
      UIAnimationManager.Instance.HideWindow(pausePanel, 0.5f);
   }
   
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public RectTransform Main_Panel;
    public RectTransform Starting_Game_Panel;

    public void StartGameButton()
    {
        UIAnimationManager.Instance.HideWindow(Main_Panel, 0.5f);
        UIAnimationManager.Instance.HideWindow(Starting_Game_Panel, 0.5f);

        FindObjectOfType<VideoController>()
            .PlayVideo(() =>
            {
                SceneManager.LoadScene("Prototype");
            });
    }
}
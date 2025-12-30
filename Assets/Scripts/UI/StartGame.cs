using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public RectTransform Main_Panel;
    public RectTransform Starting_Game_Panel;
    public AudioSource Starting_Game_Audio;

    public void StartGameButton()
    {
        UIAnimationManager.Instance.HideWindow(Main_Panel, 0.5f);
        UIAnimationManager.Instance.HideWindow(Starting_Game_Panel, 0.5f);
        Starting_Game_Audio.volume = 0;
        FindObjectOfType<VideoController>()
            .PlayVideo(() =>
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            });
    }
}
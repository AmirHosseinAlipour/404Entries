using UnityEngine;

public class StartGame : MonoBehaviour
{
    public void StartGameButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Prototype");
    }
}

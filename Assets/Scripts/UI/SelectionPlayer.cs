using UnityEngine;
using UnityEngine.UIElements;

public class SelectionPlayer : MonoBehaviour
{
    public int PlayerID = 0; 
    public static SelectionPlayer instance;
    public RectTransform SumbitButton;
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetPlayerID(int playerID)
    {
        UIAnimationManager.Instance.ShowWindow(SumbitButton , 0.5f);
        PlayerID = playerID;
    }
    
}

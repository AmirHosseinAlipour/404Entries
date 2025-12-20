using UnityEngine;
public  class MusicSingleton : MonoBehaviour
{
    public static MusicSingleton Instance { get; private set; }
    private int CurrentTrack = 0 ; 
    [SerializeField] private AudioSource[] Audioclip; 
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void  SetValue(int number)
    {
        CurrentTrack = number;
    }

    public int GetValue()
    {
        return CurrentTrack;
    }

    public void Play()
    {
        Audioclip[CurrentTrack].Play();
    }
}
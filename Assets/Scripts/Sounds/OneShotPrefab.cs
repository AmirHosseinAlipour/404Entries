using UnityEngine;

public class OneShotSound : MonoBehaviour
{
    public void Play(AudioClip clip)
    {
        AudioSource source = GetComponent<AudioSource>();
        source.clip = clip;
        source.Play();
        Destroy(gameObject, clip.length);
    }
}
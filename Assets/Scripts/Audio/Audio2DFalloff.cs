using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Audio2DFalloff : MonoBehaviour
{
    public Transform listenerTransform;
    public float maxDistance = 30f;
    public float minDistance = 1f;
    public float panWidth = 10f;
    [Tooltip("The lowest volume the sound will falloff to (0 = silent, 1 = max).")]
    [Range(0.0f, 1.0f)]
    public float minVolume = 0.0f;

    private AudioSource audioSource;
    private Transform sourceTransform;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        sourceTransform = transform;

        if (listenerTransform == null)
        {
            try
            {
                listenerTransform = Camera.main.transform;
            }
            catch
            {
                Debug.LogError("Audio2DFalloff: No listenerTransform assigned and Main Camera not found. Please assign the listener (player/camera) in the inspector.");
                this.enabled = false;
            }
        }
    }

    void Update()
    {
        if (listenerTransform == null) return;

        Vector2 listenerPos = new Vector2(listenerTransform.position.x, listenerTransform.position.y);
        Vector2 sourcePos = new Vector2(sourceTransform.position.x, sourceTransform.position.y);
        float distance = Vector2.Distance(listenerPos, sourcePos);

        float targetVolume;

        if (distance > maxDistance)
        {
            targetVolume = minVolume;
        }
        else if (distance < minDistance)
        {
            targetVolume = 1.0f;
        }
        else
        {
            float t = 1.0f - (distance - minDistance) / (maxDistance - minDistance);
            t = t * t;
            targetVolume = Mathf.Lerp(minVolume, 1.0f, t);
        }

        audioSource.volume = targetVolume;

        float xDistance = sourcePos.x - listenerPos.x;
        float pan = Mathf.Clamp(xDistance / panWidth, -1.0f, 1.0f);
        audioSource.panStereo = pan;
    }

    void OnDrawGizmos()
    {
        Vector3 center = transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, minDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, maxDistance);
    }
}
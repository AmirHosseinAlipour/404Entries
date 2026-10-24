using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SaveTrigger : MonoBehaviour
{
    public bool oneTimeUse = false;
    public float cooldown = 2f;
    public bool saveTriggerPosition = false;
    public AudioClip saveSound;

    private bool _hasTriggered = false;
    private float _lastSaveTime = -10f;

    private void Reset()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (oneTimeUse && _hasTriggered)
        {
            return;
        }

        if (Time.time - _lastSaveTime < cooldown)
        {
            return;
        }

        _lastSaveTime = Time.time;
        _hasTriggered = true;

        Vector3? targetPosition = saveTriggerPosition ? (Vector3?)transform.position : (Vector3?)other.transform.position;

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame(targetPosition);
        }

        if (saveSound != null)
        {
            AudioSource.PlayClipAtPoint(saveSound, transform.position);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 0.85f, 1f, 0.35f);
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Vector3 center = transform.position + (Vector3)col.offset;
            Gizmos.DrawCube(center, col.size);
            Gizmos.color = new Color(0f, 0.85f, 1f, 0.9f);
            Gizmos.DrawWireCube(center, col.size);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
    }
}

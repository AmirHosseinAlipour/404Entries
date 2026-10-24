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
        ProcessTrigger(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        ProcessTrigger(other);
    }

    private void ProcessTrigger(Collider2D other)
    {
        bool isPlayer = other.CompareTag("Player") || (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player"));
        if (!isPlayer)
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

        GameObject playerObj = GameObject.FindWithTag("Player");
        Vector3 targetPosition = Vector3.zero;
        if (saveTriggerPosition)
        {
            targetPosition = transform.position;
        }
        else if (playerObj != null)
        {
            targetPosition = playerObj.transform.position;
        }
        else
        {
            targetPosition = other.transform.root.position;
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame(targetPosition);
        }

        if (saveSound != null)
        {
            AudioSource.PlayClipAtPoint(saveSound, transform.position);
        }

        Debug.Log("[SaveTrigger] Game saved at: " + targetPosition);
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

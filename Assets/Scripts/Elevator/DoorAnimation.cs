using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DoorAnimation : MonoBehaviour
{
    public RectTransform leftDoor;
    public RectTransform rightDoor;
    public float speed = 800f;
    private Vector2 leftStartPos, rightStartPos;

    private void Start()
    {
        leftStartPos = leftDoor.anchoredPosition;
        rightStartPos = rightDoor.anchoredPosition;
    }

    public void CloseDoors()
    {
        StartCoroutine(MoveDoors(Vector2.zero, true));
    }

    public void OpenDoors()
    {
        StartCoroutine(MoveDoors(Vector2.zero, false));
    }

    private IEnumerator MoveDoors(Vector2 center, bool closing)
    {
        float elapsed = 0f;
        float duration = 1f;

        Vector2 leftTarget = closing ? new Vector2(0, leftStartPos.y) : leftStartPos;
        Vector2 rightTarget = closing ? new Vector2(0, rightStartPos.y) : rightStartPos;

        Vector2 leftInitial = leftDoor.anchoredPosition;
        Vector2 rightInitial = rightDoor.anchoredPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            leftDoor.anchoredPosition = Vector2.Lerp(leftInitial, leftTarget, elapsed / duration);
            rightDoor.anchoredPosition = Vector2.Lerp(rightInitial, rightTarget, elapsed / duration);
            yield return null;
        }
    }
}
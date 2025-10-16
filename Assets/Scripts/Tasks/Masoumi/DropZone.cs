using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    // This method is called automatically when a draggable item is dropped onto this object
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;
        DraggableItem draggableItem = droppedObject.GetComponent<DraggableItem>();

        if (draggableItem != null)
        {
            // Snap the dropped item to the center of this drop zone
            draggableItem.transform.SetParent(transform);
            draggableItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }
}
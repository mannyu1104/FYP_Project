using UnityEngine;
using UnityEngine.EventSystems;

// Same drag feel as the team's DragableItem (grab -> follow pointer -> release),
// but it doesn't decide where it belongs. Whichever ClueDropZone the pointer is
// over when released gets first claim (see ClueDropZone.OnDrop); if nothing
// claims it, OnEndDrag snaps it back to wherever it started.
[RequireComponent(typeof(CanvasGroup))]
public class DraggableClueEntry : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private Transform parentBeforeDrag;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentBeforeDrag = transform.parent;
        transform.SetParent(DragLayer.Transform, worldPositionStays: true);
        transform.SetAsLastSibling();

        // Turning raycasts off is what lets ClueDropZone.OnDrop "see through"
        // this item to whatever zone is underneath the pointer.
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // eventData.position works for mouse, touch and the new Input System alike,
        // unlike Input.mousePosition which only covers mouse.
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // If a ClueDropZone accepted the drop, it already re-parented this object,
        // so we're no longer sitting directly under the drag layer. If we are,
        // nothing accepted it - snap back to where it came from.
        if (transform.parent == DragLayer.Transform)
        {
            transform.SetParent(parentBeforeDrag);
        }
    }
}
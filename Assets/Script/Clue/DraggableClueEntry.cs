using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableClueEntry : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private Transform parentBeforeDrag;
    private Transform rootCanvasTransform;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvasTransform = GetComponentInParent<Canvas>().transform;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentBeforeDrag = transform.parent;
        transform.SetParent(rootCanvasTransform, worldPositionStays: true);
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
        // so we're no longer sitting directly under the canvas root. If we are,
        // nothing accepted it - snap back to where it came from.
        if (transform.parent == rootCanvasTransform)
        {
            transform.SetParent(parentBeforeDrag);
        }
    }
}
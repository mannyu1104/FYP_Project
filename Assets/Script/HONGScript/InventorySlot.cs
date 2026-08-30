using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0)
        {
            if (eventData.pointerDrag.layer == LayerMask.NameToLayer("Item"))
            {
                GameObject dropped = eventData.pointerDrag;
                DragableItem draggableitem = dropped.GetComponent<DragableItem>();
                //if (draggableitem.thisUnlocked == true && draggableitem.thisGet == true)
                //{
                //    draggableitem.parentAfterDrag = transform;
                //}
                if (draggableitem.thisGet == true)
                {
                    draggableitem.parentAfterDrag = transform;
                }
            }
            else
            {
                return;
            }
        }
    }
}

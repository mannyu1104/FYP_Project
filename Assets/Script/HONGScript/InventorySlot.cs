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
                DragableItemSave draggableitemSave = dropped.GetComponent<DragableItemSave>();
                //if (draggableitem.thisUnlocked == true && draggableitem.thisGet == true)
                //{
                //    draggableitem.parentAfterDrag = transform;
                //}
                if (draggableitemSave != null)
                {
                    if (draggableitemSave.thisGet == true)
                    {
                        draggableitemSave.parentAfterDrag = transform;
                    }
                }
                else if (draggableitem != null)
                {
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
            else
            {
                return;
            }
        }
    }
}

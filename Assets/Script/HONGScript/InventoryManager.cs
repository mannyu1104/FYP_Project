using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] inventorySlots;
    //public GameObject InventoryItemPrefab;
    public void AddItem(GameObject item)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            DragableItem iteminslot = slot.GetComponentInChildren<DragableItem>();
            if (iteminslot == null)
            {
                SetNewItem(item, slot);
                return;
            }
        }
    }

    void SetNewItem(GameObject item, InventorySlot slot)
    {
        DragableItem dragableitem = item.GetComponent<DragableItem>();
        dragableitem.thisGet = true;
        dragableitem.parentAfterDrag = slot.transform;
    }

    //void SetNewItem(Item item, InventorySlot slot)
    //{
    //    GameObject newItemGO = Instantiate(InventoryItemPrefab, slot.transform);
    //    DragableItem dragableitem = newItemGO.GetComponent<DragableItem>();
    //    dragableitem.InitialiseItem(item);
    //}
}

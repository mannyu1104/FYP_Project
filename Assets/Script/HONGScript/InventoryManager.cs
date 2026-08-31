using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] inventorySlotsForMap;
    public InventorySlot[] inventorySlotsForTutorial;
    public InventorySlot[] inventorySlotsForIngame;

    public DragableItem Item;
    public DragableItemSave MapItem;

    //public GameObject InventoryItemPrefab;
    public void AddItem(GameObject item)
    {
        Item = item.GetComponent<DragableItem>();
        MapItem = item.GetComponent<DragableItemSave>();
        if (Item != null )
        {
            if (Item.thisType == "Tutorial")
            {
                for (int i = 0; i < inventorySlotsForTutorial.Length; i++)
                {
                    InventorySlot slot = inventorySlotsForTutorial[i];
                    DragableItem iteminslot = slot.GetComponentInChildren<DragableItem>();
                    if (iteminslot == null)
                    {
                        SetNewItem(item, slot);
                        return;
                    }
                }
            }
            else if (Item.thisType == "Ingame")
            {
                for (int i = 0; i < inventorySlotsForIngame.Length; i++)
                {
                    InventorySlot slot = inventorySlotsForIngame[i];
                    DragableItem iteminslot = slot.GetComponentInChildren<DragableItem>();
                    if (iteminslot == null)
                    {
                        SetNewItem(item, slot);
                        return;
                    }
                }
            }
            else
            {
                Debug.Log("WrongType");
                return;
            }
        }
        else if (MapItem != null)
        {
            if (MapItem.thisType == "Map")
            {
                for (int i = 0; i < inventorySlotsForMap.Length; i++)
                {
                    InventorySlot slot = inventorySlotsForMap[i];
                    DragableItemSave iteminslot = slot.GetComponentInChildren<DragableItemSave>();
                    if (iteminslot == null)
                    {
                        SetNewItemMap(item, slot);
                        return;
                    }
                }
            }
            else
            {
                Debug.Log("WrongType");
                return;
            }
        }
        else
        {
            Debug.Log("WrongType");
            return;
        }
    }

    void SetNewItem(GameObject item, InventorySlot slot)
    {
        DragableItem dragableitem = item.GetComponent<DragableItem>();

        if (dragableitem.thisGet == false)
        {
            dragableitem.thisGet = true;
        }
        if (dragableitem.thisUsed == true)
        {
            dragableitem.thisUsed = false;
        }
        dragableitem.parentAfterDrag = slot.transform;
        if (dragableitem.isdragging == false)
        {
            dragableitem.LoadSetParent();
        }
        Debug.Log("Adding");
    }

    void SetNewItemMap(GameObject item, InventorySlot slot)
    {
        DragableItemSave mapitem = item.GetComponent<DragableItemSave>();

        if (mapitem.thisGet == false)
        {
            mapitem.thisGet = true;
        }
        if (mapitem.thisUsed == true)
        {
            mapitem.thisUsed = false;
        }
        mapitem.parentAfterDrag = slot.transform;
        if (mapitem.isdragging == false)
        {
            mapitem.LoadSetParent();
        }
        Debug.Log("Adding");
    }

    //void SetNewItem(Item item, InventorySlot slot)
    //{
    //    GameObject newItemGO = Instantiate(InventoryItemPrefab, slot.transform);
    //    DragableItem dragableitem = newItemGO.GetComponent<DragableItem>();
    //    dragableitem.InitialiseItem(item);
    //}
}

using UnityEngine;
using System;
using System.Collections.Generic;

//[System.Serializable]
//public class InventorySaveData
//{
//    public List<ItemBooleanData> ItemBool = new List<ItemBooleanData>();
//}

//[System.Serializable]
//public class ItemBooleanData
//{
//    public int ItemID;
//    public bool Unlocked;
//    public bool Get;
//    public bool Used;
//}

//public class ItemData: MonoBehaviour
//{
//    public static ItemData instance;

//    public List<Item> allItems; 

//    private Dictionary<int, Item> itemLookup = new Dictionary<int, Item>(); 

//    public void Awake()
//    {
//        instance = this;

//        itemLookup.Clear();

//        foreach (Item item in allItems)
//        {
//            itemLookup[item.ItemID] = item;
//        }
//    }

//    public Item GetItem(int id)
//    {
//        return itemLookup.ContainsKey(id) ? itemLookup[id] : null;
//    }
//}

//public bool Unlocked;
//public bool Get;
//public bool Used;

//public ItemData(DragableItem item)
//{
//    Unlocked = item.thisUnlocked;
//    Get = item.thisGet;
//    Used = item.thisUsed;
//}

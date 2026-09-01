using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

[System.Serializable]
public class InventorySaveData
{
    public List<ItemBooleanData> ItemBool = new List<ItemBooleanData>();
}

[System.Serializable]
public class ItemBooleanData
{
    public int ItemID;
    public bool Show;
    public bool Get;
    public bool Used;
    public string ItemShowName;
    public string Description;
    public string TypeofItem;
}

//[System.Serializable]
//public class MapSaveData
//{
//    public List<MapBooleanData> MapBool = new List<MapBooleanData>();
//}

//[System.Serializable]
//public class MapBooleanData
//{
//    public int ItemID;
//    public bool Show;
//    public bool Get;
//    public bool Used;
//    public string ItemShowName;
//    public string Description;
//    public string TypeofItem;
//}

//[System.Serializable]
//public class ItemLockSaveData
//{
//    public List<ItemLockBooleanData> MapBool = new List<ItemLockBooleanData>();
//}

//[System.Serializable]
//public class ItemLockBooleanData
//{
//    public int ItemID;
//    public bool Show;
//    public bool Get;
//    public bool Used;
//    public string ItemShowName;
//    public string Description;
//    public string TypeofItem;
//}

public class SaveSystem: MonoBehaviour
{
    public static SaveSystem instance;
    public InventoryManager inventorymanager;
    public InventoryManagerINFO inventoryUsing;

    private string path;
    private string pathmap;
    private string pathitemlock;

    private void Awake()
    {
        instance = this;

        path = Application.persistentDataPath + "/inventory.json";
        pathmap = Application.persistentDataPath + "/map.json";
        pathitemlock = Application.persistentDataPath + "/inventorylock.json";
    }

    public void SaveInventory()
    {
        InventorySaveData data = new InventorySaveData();

        DragableItem[] items = FindObjectsByType<DragableItem>();

        foreach (DragableItem item in items)
        {
            ItemBooleanData dataItem = new ItemBooleanData();

            dataItem.ItemID = item.thisID;
            //dataItem.Show = item.thisShow;
            dataItem.Get = item.thisGet;
            dataItem.Used = item.thisUsed;
            dataItem.Description = item.thisDescription;
            dataItem.ItemShowName = item.thisName;
            dataItem.TypeofItem = item.thisType;

            data.ItemBool.Add(dataItem);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
    }

    public void LoadGame()
    {
        if (!File.Exists(path))
        {
            return;
        }

        string json = File.ReadAllText(path);
        InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(json);

        DragableItem[] items = FindObjectsByType<DragableItem>();

        foreach (ItemBooleanData dataItem in data.ItemBool)
        {
            foreach (DragableItem item in items)
            {
                if (item.thisID == dataItem.ItemID)
                {
                    //item.thisShow = dataItem.Show;
                    item.thisGet = dataItem.Get;
                    item.thisUsed = dataItem.Used;

                    break;
                }
            } 
        }

        PutInInventory();
    }
    private void PutInInventory()
    {
        DragableItem[] itemsload = FindObjectsByType<DragableItem>();
        foreach (DragableItem putitem in itemsload)
        {
            if (putitem.thisUsed == true)
            {
                inventoryUsing.AddItem(putitem.gameObject);
            }
            else if (putitem.thisUsed == false && putitem.thisGet == true)
            {
                inventorymanager.AddItem(putitem.gameObject);
            }
        }
    }
}

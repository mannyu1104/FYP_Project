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
    //public bool Show;
    public bool Get;
    public bool Used;
    //public string ItemShowName;
    //public string Description;
    //public string TypeofItem;
}

[System.Serializable]
public class MapSaveData
{
    public List<MapBooleanData> MapBool = new List<MapBooleanData>();
}

[System.Serializable]
public class MapBooleanData
{
    public int MapID;
    public bool Unlocked;
    //public bool Show;
    //public bool Get;
    //public bool Used;
    //public string ItemShowName;
    //public string Description;
    //public string TypeofItem;
}

[System.Serializable]
public class ItemLockSaveData
{
    public List<ItemLockBooleanData> ItemLockBool = new List<ItemLockBooleanData>();
}

[System.Serializable]
public class ItemLockBooleanData
{
    public int ItemID;
    public bool Show;
    public bool Get;
    public bool Used;
    //public string ItemShowName;
    //public string Description;
    //public string TypeofItem;
}

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
            //dataItem.Description = item.thisDescription;
            //dataItem.ItemShowName = item.thisName;
            //dataItem.TypeofItem = item.thisType;

            data.ItemBool.Add(dataItem);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
    }

    public void SaveInventoryMap()
    {
        ItemLockSaveData data = new ItemLockSaveData();

        DragableItemSave[] items = FindObjectsByType<DragableItemSave>();

        foreach (DragableItemSave item in items)
        {
            ItemLockBooleanData dataItem = new ItemLockBooleanData();

            dataItem.ItemID = item.thisID;
            dataItem.Show = item.thisShow;
            dataItem.Get = item.thisGet;
            dataItem.Used = item.thisUsed;
            //dataItem.Description = item.thisDescription;
            //dataItem.ItemShowName = item.thisName;
            //dataItem.TypeofItem = item.thisType;

            data.ItemLockBool.Add(dataItem);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(pathitemlock, json);
    }

    public void SaveMap()
    {
        MapSaveData data = new MapSaveData();

        MapIcon[] items = FindObjectsByType<MapIcon>();

        foreach (MapIcon item in items)
        {
            MapBooleanData dataItem = new MapBooleanData();

            dataItem.MapID = item.thisID;
            dataItem.Unlocked = item.thisUnlocked;
            //dataItem.Show = item.thisShow;
            //dataItem.Get = item.thisGet;
            //dataItem.Used = item.thisUsed;
            //dataItem.Description = item.thisDescription;
            //dataItem.ItemShowName = item.thisName;
            //dataItem.TypeofItem = item.thisType;

            data.MapBool.Add(dataItem);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(pathmap, json);
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

    public void LoadGameItemLock()
    {
        if (!File.Exists(pathitemlock))
        {
            return;
        }

        string json = File.ReadAllText(pathitemlock);
        ItemLockSaveData data = JsonUtility.FromJson<ItemLockSaveData>(json);

        DragableItemSave[] items = FindObjectsByType<DragableItemSave>();

        foreach (ItemLockBooleanData dataItem in data.ItemLockBool)
        {
            foreach (DragableItemSave item in items)
            {
                if (item.thisID == dataItem.ItemID)
                {
                    item.thisShow = dataItem.Show;
                    item.thisGet = dataItem.Get;
                    item.thisUsed = dataItem.Used;

                    break;
                }
            }
        }

        PutInInventoryMapItem();
    }

    public void LoadGameMap()
    {
        if (!File.Exists(pathmap))
        {
            return;
        }

        string json = File.ReadAllText(pathmap);
        MapSaveData data = JsonUtility.FromJson<MapSaveData>(json);

        MapIcon[] items = FindObjectsByType<MapIcon>();

        foreach (MapBooleanData dataItem in data.MapBool)
        {
            foreach (MapIcon item in items)
            {
                if (item.thisID == dataItem.MapID)
                {
                    //item.thisShow = dataItem.Show;
                    //item.thisGet = dataItem.Get;
                    //item.thisUsed = dataItem.Used;
                    item.thisUnlocked = dataItem.Unlocked;

                    break;
                }
            }
        }

        PutInInventoryMap();
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
                putitem.thisGet = false;
                inventorymanager.AddItem(putitem.gameObject);
            }
        }
    }

    private void PutInInventoryMapItem()
    {
        DragableItemSave[] itemsload = FindObjectsByType<DragableItemSave>();
        foreach (DragableItemSave putitem in itemsload)
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

    private void PutInInventoryMap()
    {
        MapIcon[] itemsload = FindObjectsByType<MapIcon>();
        foreach (MapIcon putitem in itemsload)
        {
            if (putitem.thisUnlocked == true)
            {
                putitem.Unlocking();
            }
            else if (putitem.thisUnlocked == false)
            {
                putitem.NotUnlock();
            }
        }
    }

}

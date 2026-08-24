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
    public bool Unlocked;
    public bool Get;
    public bool Used;
}

public class SaveSystem: MonoBehaviour
{
    public static SaveSystem instance;
    public InventoryManager inventorymanager;
    public InventoryManagerINFO inventoryUsing;

    private string path;

    private void Awake()
    {
        instance = this;

        path = Application.persistentDataPath + "/inventory.json";


    }

    public void SaveInventory()
    {
        InventorySaveData data = new InventorySaveData();

        DragableItem[] items = FindObjectsByType<DragableItem>();

        foreach (DragableItem item in items)
        {
            ItemBooleanData dataItem = new ItemBooleanData();

            dataItem.ItemID = item.thisID;
            dataItem.Unlocked = item.thisUnlocked;
            dataItem.Get = item.thisGet;
            dataItem.Used = item.thisUsed;

            data.ItemBool.Add(dataItem);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);

        Debug.Log("Save to" + path);
        //foreach (Slot slot in DragableItem.instance.GetAllSlots())
        //{

        //}
    }

    public void LoadGame()
    {
        if (!File.Exists(path))
        {
            Debug.Log("No Save File Found");
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
                    item.thisUnlocked = dataItem.Unlocked;
                    item.thisGet = dataItem.Get;
                    item.thisUsed = dataItem.Used;

                    break;
                }
            } 
        }

        PutInInventory();
        Debug.Log("GameLoad");
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
                Debug.Log("Getload");
                GameObject ItemPut = putitem.gameObject;
                inventorymanager.AddItem(ItemPut);
            }
            else
            {
                return;
            }
        }
    }
}

using System;
using Unity.VisualScripting;
using UnityEngine;

public class SummaryButton : MonoBehaviour
{
    public GameObject[] SaveIntObject;
    public InventoryManager inventory;
    public string NowTitle;

    private void OnEnable()
    {
        // Only spawn clues we haven't seen before. This is deliberate: if the
        // player already dragged some clues into the judgment zone and then
        // leaves to question another witness, coming back here must NOT reset
        // their sorting progress.
        ClueManager.Instance.OnClueRecorded += GetTitle;
    }

    private void OnDisable()
    {
        if (ClueManager.Instance != null)
        {
            ClueManager.Instance.OnClueRecorded -= GetTitle;
        }
    }

    public void GetTitle(ClueManager.RecordedClue clue)
    {
        NowTitle = clue.title.GetLocalizedString();
        Debug.Log(NowTitle);
        BecomeInvet();
    }


    public void BecomeInvet()
    {
        DragableItem[] items = FindObjectsByType<DragableItem>();

        foreach (DragableItem item in items)
        {
            if (string.Equals(NowTitle, item.thisName, System.StringComparison.OrdinalIgnoreCase))
            {
                int WhatID = item.thisID;
                inventory.AddItem(SaveIntObject[WhatID]);
                Debug.Log("RIGHTADDING");
                return;
            }

            else
            {
                Debug.Log(item.thisName);
            }   
        }
    }

}

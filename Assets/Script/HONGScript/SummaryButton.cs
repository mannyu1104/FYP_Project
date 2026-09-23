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
        if (ClueManager.Instance != null)
        {
            ClueManager.Instance.OnClueRecorded += GetTitle;
        }
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
        if (clue == null || clue.title == null)
        {
            return;
        }

        try
        {
            var initHandle = UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
            if (!initHandle.IsDone)
            {
                return;
            }

            NowTitle = clue.title.GetLocalizedString() ?? string.Empty;
        }
        catch
        {
            NowTitle = string.Empty;
            return;
        }

        Debug.Log(NowTitle);
        BecomeInvet();
    }


    public void BecomeInvet()
    {
        if (inventory == null)
        {
            Debug.LogWarning("SummaryButton inventory reference is missing.", this);
            return;
        }

        if (SaveIntObject == null || SaveIntObject.Length == 0)
        {
            Debug.LogWarning("SummaryButton SaveIntObject is missing.", this);
            return;
        }

        DragableItem[] items = FindObjectsByType<DragableItem>();

        Debug.Log(gameObject.name);
        foreach (DragableItem item in items)
        {
            if (item == null)
            {
                continue;
            }

            if (string.Equals(NowTitle, item.thisName, System.StringComparison.OrdinalIgnoreCase))
            {
                int WhatID = item.thisID;
                if (WhatID < 0 || WhatID >= SaveIntObject.Length || SaveIntObject[WhatID] == null)
                {
                    Debug.LogWarning($"SummaryButton cannot add clue item for ID {WhatID}.", this);
                    return;
                }

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

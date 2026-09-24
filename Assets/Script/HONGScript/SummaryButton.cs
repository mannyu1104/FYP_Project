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

        if (inventory == null) inventory = GetComponentInParent<InventoryManager>();
        if (inventory == null) return;
        // The old list contains only tutorial clues. Include the authored news and
        // social-media clue objects in this same inventory prefab, even while hidden.
        foreach (var item in transform.root.GetComponentsInChildren<DragableItem>(true))
        {
            if (item == null) continue;
            var obj = item.gameObject;
            item.EnsureInitialized();
            var title = item.dragSourceData != null ? item.dragSourceData.ClueTitle : null;
            bool matches = title != null && title.TableReference.Equals(clue.title.TableReference) &&
                title.TableEntryReference.Equals(clue.title.TableEntryReference);
            if (!matches && title == null)
            {
                var init = UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
                if (init.IsDone) matches = string.Equals(clue.title.GetLocalizedString(), item.thisName, StringComparison.OrdinalIgnoreCase);
            }
            if (!matches) continue;
            if (item.thisGet || item.thisUsed) return;
            inventory.AddItem(obj);
            if (item.thisGet) obj.SetActive(true);
            return;
        }
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

        DragableItem[] items = FindObjectsByType<DragableItem>(FindObjectsInactive.Include);

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

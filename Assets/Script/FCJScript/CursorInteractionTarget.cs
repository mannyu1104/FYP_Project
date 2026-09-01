using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;

public class CursorInteractionTarget : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector]
    [Tooltip("Choose an existing cursor preset from the Cursor Manager. Do not type manually.")]
    public string cursorPresetName;

    [Header("Item Inspect")]
    [Tooltip("Enable the item introduction dialogue when this object is clicked.")]
    public bool enableInspectDialogue = true;
    // Legacy string field kept as a reminder of the previous non-localized format.
    // public string itemName = "Item";
    public LocalizedString itemName;
    // public string itemDescription = "This is an item.";
    public List<LocalizedString> itemDescriptionLines = new List<LocalizedString>();
    public Sprite itemSprite;
    public bool showInspectImage = true;

    [HideInInspector]
    public string customCursorPresetName;
    [HideInInspector]
    public int customCursorPresetIndex;
    [HideInInspector]
    public int interactionType;
    [HideInInspector]
    public bool useCustomPreset;
    [HideInInspector]
    public int cursorPresetIndex;
    [HideInInspector]
    public DialogueController dialogueController;

    public void TriggerInspection()
    {
        if (WhiteBoard.IsAnyWhiteBoardOpen)
        {
            return;
        }

        if (!enableInspectDialogue)
        {
            return;
        }

        if (dialogueController == null)
        {
            dialogueController = FindAnyObjectByType<DialogueController>();
        }

        if (dialogueController != null)
        {
            dialogueController.ShowItemInspect(this);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TriggerInspection();
    }
}

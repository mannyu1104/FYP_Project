using UnityEngine;
using UnityEngine.EventSystems;

public class CursorInteractionTarget : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector]
    [Tooltip("Choose an existing cursor preset from the Cursor Manager. Do not type manually.")]
    public string cursorPresetName;

    [Header("Item Inspect")]
    [Tooltip("Enable the item introduction dialogue when this object is clicked.")]
    public bool enableInspectDialogue = true;
    public string itemName = "Item";
    [TextArea(2, 6)]
    public string itemDescription = "This is an item.";
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
        if (!enableInspectDialogue)
        {
            return;
        }

        if (dialogueController == null)
        {
            dialogueController = FindFirstObjectByType<DialogueController>();
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

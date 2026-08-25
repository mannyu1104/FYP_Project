using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NPCDialogueTrigger : MonoBehaviour, IPointerClickHandler
{
    [Header("Dialogue")]
    public DialogueController dialogueController;
    public List<DialogueController.DialogueLine> dialogueLines = new List<DialogueController.DialogueLine>();
    public bool allowRepeatConversation = true;

    [Header("Talked State")]
    [SerializeField]
    private bool hasTalked;

    public bool HasTalked => hasTalked;

    public void OnPointerClick(PointerEventData eventData)
    {
        StartDialogue();
    }

    void OnMouseDown()
    {
        StartDialogue();
    }

    public void StartDialogue()
    {
        if (dialogueController == null)
        {
            dialogueController = FindFirstObjectByType<DialogueController>();
        }

        if (dialogueController == null || dialogueLines.Count == 0)
        {
            return;
        }

        if (dialogueController.IsDialogueActive || (!allowRepeatConversation && hasTalked))
        {
            return;
        }

        dialogueController.StartConversation(dialogueLines, this);
    }

    public void MarkTalked()
    {
        hasTalked = true;
    }
}

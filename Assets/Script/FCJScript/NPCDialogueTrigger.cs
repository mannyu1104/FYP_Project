using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NPCDialogueTrigger : MonoBehaviour, IPointerClickHandler
{
    [System.Serializable]
    public class DialogueConversation
    {
        public string conversationName = "Conversation";
        public List<DialogueController.DialogueLine> dialogueLines = new List<DialogueController.DialogueLine>();
    }

    [Header("Dialogue")]
    [Tooltip("Use a unique ID for this NPC. The ID must stay the same between game sessions.")]
    public string saveId;
    public DialogueController dialogueController;
    [Tooltip("Add one conversation for each time the player should talk to this NPC.")]
    public List<DialogueConversation> conversations = new List<DialogueConversation>();
    [HideInInspector]
    public List<DialogueController.DialogueLine> dialogueLines = new List<DialogueController.DialogueLine>();
    public bool allowRepeatConversation = true;

    [Header("Talked State")]
    [SerializeField]
    private bool hasTalked;
    [SerializeField]
    private int nextConversationIndex;

    public bool HasTalked => hasTalked;
    public int NextConversationIndex => nextConversationIndex;
    public string SaveId => string.IsNullOrWhiteSpace(saveId) ? gameObject.name : saveId;

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
        if (WhiteBoard.IsAnyWhiteBoardOpen)
        {
            return;
        }

        if (dialogueController == null)
        {
            dialogueController = FindAnyObjectByType<DialogueController>();
        }

        List<DialogueController.DialogueLine> lines = GetNextConversationLines();

        if (dialogueController == null || lines == null || lines.Count == 0)
        {
            return;
        }

        if (dialogueController.IsDialogueActive || (!allowRepeatConversation && hasTalked && conversations.Count == 0))
        {
            return;
        }

        dialogueController.StartConversation(lines, this);
    }

    public void MarkTalked()
    {
        hasTalked = true;

        if (conversations.Count > 0 && nextConversationIndex < conversations.Count)
        {
            nextConversationIndex++;
        }
    }

    public void RestoreProgress(int conversationIndex, bool talked)
    {
        nextConversationIndex = Mathf.Max(0, conversationIndex);
        hasTalked = talked;
    }

    public void ResetProgress()
    {
        nextConversationIndex = 0;
        hasTalked = false;
    }

    private List<DialogueController.DialogueLine> GetNextConversationLines()
    {
        if (conversations.Count == 0)
        {
            return dialogueLines;
        }

        if (nextConversationIndex >= conversations.Count)
        {
            return allowRepeatConversation
                ? conversations[conversations.Count - 1].dialogueLines
                : null;
        }

        return conversations[nextConversationIndex].dialogueLines;
    }
}

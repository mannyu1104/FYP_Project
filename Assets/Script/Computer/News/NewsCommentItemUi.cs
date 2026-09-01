using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NewsCommentItemUi : MonoBehaviour
{
    [SerializeField] private TMP_Text commenterNameText;
    [SerializeField] private TMP_InputField commentText;

    private ScrollRect parentScrollRect;
    private NewsCommentEntry commentEntry;

    private void Awake()
    {
        parentScrollRect = GetComponentInParent<ScrollRect>();

        SetupScrollForwarding(commentText);
    }

    private void SetupScrollForwarding(TMP_InputField inputField)
    {
        if (inputField == null)
            return;

        EventTrigger trigger = inputField.GetComponent<EventTrigger>();

        if (trigger == null)
        {
            trigger = inputField.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry scrollEntry = new EventTrigger.Entry();
        scrollEntry.eventID = EventTriggerType.Scroll;

        scrollEntry.callback.AddListener((data) =>
        {
            PointerEventData pointerData = (PointerEventData)data;

            if (parentScrollRect != null)
            {
                parentScrollRect.OnScroll(pointerData);
            }
        });

        trigger.triggers.Add(scrollEntry);
    }

    public void Bind(NewsCommentEntry comment, NewsPageController owner)
    {
        UnsubscribeFromLocalization();
        commentEntry = comment;
        SubscribeToLocalization();
    }

    private void SubscribeToLocalization()
    {
        if (commentEntry == null) return;
        commentEntry.name.StringChanged += UpdateCommenterNameText;
        commentEntry.commentText.StringChanged += UpdateCommentText;
    }

    private void UnsubscribeFromLocalization()
    {
        if (commentEntry == null) return;
        commentEntry.name.StringChanged -= UpdateCommenterNameText;
        commentEntry.commentText.StringChanged -= UpdateCommentText;
    }

    private void UpdateCommenterNameText(string value)
    {
        if (commenterNameText != null) commenterNameText.text = value;
    }

    private void UpdateCommentText(string value)
    {
        if (commentText != null) commentText.text = value;
    }

    private void OnDestroy()
    {
        UnsubscribeFromLocalization();
    }
}

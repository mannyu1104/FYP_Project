using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NewsCommentItemUi : MonoBehaviour
{
    [SerializeField] private TMP_Text commenterNameText;
    [SerializeField] private TMP_InputField commentText;

    private ScrollRect parentScrollRect;

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
        commenterNameText.text = comment.name;
        commentText.text = comment.commentText;
    }
}

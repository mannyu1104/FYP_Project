using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Comment list item prefab, one row in the list of comments. 
public class SocialCommentItemUI : MonoBehaviour
{
    [SerializeField] private CustomButtonUi commenterAvatarButton;
    [SerializeField] private CustomButtonUi commenterNameButton;
    [SerializeField] private Image commenterAvatarImage;
    [SerializeField] private TMP_Text commenterNameText;
    [SerializeField] private TMP_InputField commentText;

    [Header("Name Color")]
    [SerializeField] private Color normalNameColor;
    [SerializeField] private Color hasProfileNameColor;

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

    public void Bind(SocialCommentEntry comment, SocialMediaPageController owner)
    {
        commenterAvatarImage.sprite = comment.account.Avatar;
        commenterNameText.text = comment.account.AccountName;
        commentText.text = comment.commentText;

        commenterAvatarButton.interactable = comment.hasProfilePage;
        if (comment.hasProfilePage)
        {
            commenterNameText.color = hasProfileNameColor;

            commenterAvatarButton.onLeftClick.RemoveAllListeners();
            commenterAvatarButton.onLeftClick.AddListener(() => owner.ShowProfile(comment.account));
            commenterNameButton.onLeftClick.RemoveAllListeners();
            commenterNameButton.onLeftClick.AddListener(() => owner.ShowProfile(comment.account));
        }
        else
        {
            commenterNameText.color = normalNameColor;
        }
    }
}
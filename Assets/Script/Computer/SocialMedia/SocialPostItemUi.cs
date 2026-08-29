using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Post item UI for the social media page. 
public class SocialPostItemUI : MonoBehaviour
{
    [Header("Poster Header")]
    [SerializeField] private Image posterAvatarImage;
    [SerializeField] private TMP_Text posterNameText;

    [Header("Content")]
    [SerializeField] private TMP_InputField contentText;
    [SerializeField] private GameObject imageContainer; // toggled on/off depending on whether the post has image or not
    [SerializeField] private Image postImageDisplay;

    [Header("Comments")]
    [SerializeField] private Transform commentContainer; 
    [SerializeField] private SocialCommentItemUI commentItemPrefab;

    [Header("Clue")]
    [SerializeField] private ClueRecordButton clueRecordButton;

    private readonly List<GameObject> spawnedComments = new List<GameObject>();
    private SocialMediaPageController owner;
    private ScrollRect parentScrollRect;

    private void Awake()
    {
        parentScrollRect = GetComponentInParent<ScrollRect>();

        SetupScrollForwarding(contentText);
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

    public void Bind(SocialPostData post, SocialMediaPageController owner)
    {
        this.owner = owner;

        posterAvatarImage.sprite = post.Account.Avatar;
        posterNameText.text = post.Account.AccountName;
        contentText.text = post.Content;

        bool hasImage = post.PostImage != null;
        imageContainer.SetActive(hasImage);
        if (hasImage)
        {
            postImageDisplay.sprite = post.PostImage;
        }

        clueRecordButton.SetSource(post);

        BuildComments(post);
    }

    private void BuildComments(SocialPostData post)
    {
        foreach (GameObject item in spawnedComments)
            Destroy(item);
        spawnedComments.Clear();

        foreach (SocialCommentEntry comment in post.Comments)
        {
            SocialCommentItemUI item = Instantiate(commentItemPrefab, commentContainer);
            item.Bind(comment, owner);
            spawnedComments.Add(item.gameObject);
        }
    }
}
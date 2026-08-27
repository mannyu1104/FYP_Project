using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the Social Media page's two views: Feed and Profile. There is no
/// detail page - each row (SocialPostPreviewUI) already shows its full
/// content and comments, in both the feed and the profile list. Clicking a
/// post's poster icon switches to that poster's profile; the profile's
/// back button returns to the feed. Only one level deep, so no history
/// stack is needed (unlike an earlier version of this file).
///
/// feedPosts and profilePosts are fixed, manually-assigned lists - drag the
/// relevant SocialPostData assets into each in the Inspector. Nothing is
/// filtered or computed at runtime.
///
/// Entirely separate from BrowserTabManager - none of this creates new tabs,
/// it only changes what is shown inside the Social Media tab's content area.
/// </summary>
public class SocialMediaPageController : MonoBehaviour
{
    [Header("Feed Data")]
    [SerializeField] private List<SocialPostData> feedPosts;

    [Header("Feed View")]
    [SerializeField] private CanvasGroup feedCanvasGroup;
    [SerializeField] private LayoutElement feedLayoutElement; // for the scroll rect to size correctly
    [SerializeField] private Transform feedContainer; // parent with a Vertical Layout Group
    [SerializeField] private SocialPostItemUI postPrefab;

    [Header("Profile Data")]
    [SerializeField] private List<SocialPostData> profilePosts;

    [Header("Profile View")]
    [SerializeField] private CanvasGroup profileCanvasGroup;
    [SerializeField] private LayoutElement profileLayoutElement; // for the scroll rect to size correctly
    [SerializeField] private Image profileAvatarImage;
    [SerializeField] private TMP_Text profileNameText;
    [SerializeField] private TMP_Text profileBioText;
    [SerializeField] private Transform profilePostsContainer; // parent with a Vertical Layout Group
    [SerializeField] private CustomButtonUi profileBackButton;

    private void Awake()
    {
        profileBackButton.onLeftClick.AddListener(ShowFeed);

        BuildFeedOnce();
        BuildProfilePostsOnce();

        // Both containers get laid out once, right here at scene start,
        // while both views are still active - long before the player can
        // ever open this tab.
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(feedContainer.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(profilePostsContainer.GetComponent<RectTransform>());

        Show(feedCanvasGroup, feedLayoutElement);
        Hide(profileCanvasGroup, profileLayoutElement);
    }

    public void ShowFeed()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(feedContainer.GetComponent<RectTransform>());

        Hide(profileCanvasGroup, profileLayoutElement);
        Show(feedCanvasGroup, feedLayoutElement);
    }

    public void ShowProfile(SocialAccountData poster)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(profilePostsContainer.GetComponent<RectTransform>());

        profileAvatarImage.sprite = poster.Avatar;
        profileNameText.text = poster.AccountName;
        profileBioText.text = poster.Bio;

        Hide(feedCanvasGroup, feedLayoutElement);
        Show(profileCanvasGroup, profileLayoutElement);
    }

    private void Show(CanvasGroup group, LayoutElement layoutElement)
    {
        layoutElement.ignoreLayout = false;

        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    private void Hide(CanvasGroup group, LayoutElement layoutElement)
    {
        layoutElement.ignoreLayout = true;

        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    private void BuildFeedOnce()
    {
        foreach (SocialPostData post in feedPosts)
        {
            SocialPostItemUI item = Instantiate(postPrefab, feedContainer);
            item.Bind(post, this);
        }
    }

    private void BuildProfilePostsOnce()
    {
        foreach (SocialPostData post in profilePosts)
        {
            SocialPostItemUI item = Instantiate(postPrefab, profilePostsContainer);
            item.Bind(post, this);
        }
    }
}
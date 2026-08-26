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
    [SerializeField] private GameObject feedView;
    [SerializeField] private Transform feedContainer; // parent with a Vertical Layout Group
    [SerializeField] private SocialPostUI postPrefab;

    [Header("Profile Data")]
    [SerializeField] private List<SocialPostData> profilePosts;

    //[Header("Profile View")]
    //[SerializeField] private GameObject profileView;
    //[SerializeField] private Image profileAvatarImage;
    //[SerializeField] private TMP_Text profileNameText;
    //[SerializeField] private TMP_Text profileBioText;
    //[SerializeField] private Transform profilePostsContainer; // parent with a Vertical Layout Group
    //[SerializeField] private CustomButtonUi profileBackButton;

    private void Awake()
    {
        //profileBackButton.onLeftClick.AddListener(ShowFeed);

        BuildFeedOnce();
        BuildProfilePostsOnce();
    }

    private void OnEnable()
    {
        // Every time the player (re)opens the Social Media tab, start back
        // at the feed - same reasoning as NewsPageController.
        ShowFeed();
    }

    public void ShowFeed()
    {
        feedView.SetActive(true);
        //profileView.SetActive(false);
    }

    public void ShowProfile(SocialAccountData poster)
    {
        feedView.SetActive(false);
        //profileView.SetActive(true);

        //profileAvatarImage.sprite = poster.Avatar;
        //profileNameText.text = poster.AccountName;
        //profileBioText.text = poster.Bio;
        // profilePostsContainer's rows were already built once in Awake -
        // nothing to rebuild here, since the list is fixed.
    }

    private void BuildFeedOnce()
    {
        foreach (SocialPostData post in feedPosts)
        {
            SocialPostUI item = Instantiate(postPrefab, feedContainer);
            item.Bind(post, this);
        }
    }

    private void BuildProfilePostsOnce()
    {
        foreach (SocialPostData post in profilePosts)
        {
            //SocialPostUI item = Instantiate(postPrefab, profilePostsContainer);
            //item.Bind(post, this);
        }
    }
}
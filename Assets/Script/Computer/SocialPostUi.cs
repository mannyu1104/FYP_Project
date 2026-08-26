using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// One full post row - used identically in the main feed and in the profile
/// page's post list. Shows the poster's icon/name (display only - the
/// post's own poster is NOT clickable), the post's content, and all of its
/// comments. Each comment's own commenter icon/name IS clickable and opens
/// that commenter's profile - see SocialCommentItemUI.
/// </summary>
public class SocialPostUI : MonoBehaviour
{
    [Header("Poster Header")]
    [SerializeField] private Image posterAvatarImage;
    [SerializeField] private TMP_Text posterNameText;

    [Header("Content")]
    [SerializeField] private TMP_Text contentText;
    [SerializeField] private GameObject imageContainer; // toggled on/off depending on whether the post has image or not
    [SerializeField] private Image postImageDisplay;

    [Header("Comments")]
    [SerializeField] private Transform commentContainer; // parent with a Vertical Layout Group
    [SerializeField] private SocialCommentItemUI commentItemPrefab;

    private readonly List<GameObject> spawnedComments = new List<GameObject>();
    private SocialMediaPageController owner;

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
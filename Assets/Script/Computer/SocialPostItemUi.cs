using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Post item UI for the social media page. 
public class SocialPostItemUI : MonoBehaviour
{
    [Header("Poster Header")]
    [SerializeField] private Image posterAvatarImage;
    [SerializeField] private TMP_Text posterNameText;

    [Header("Content")]
    [SerializeField] private TMP_Text contentText;
    [SerializeField] private GameObject imageContainer; // toggled on/off depending on whether the post has image or not
    [SerializeField] private Image postImageDisplay;

    [Header("Comments")]
    [SerializeField] private Transform commentContainer; 
    [SerializeField] private SocialCommentItemUI commentItemPrefab;

    [Header("Clue")]
    [SerializeField] private ClueRecordButton clueRecordButton;

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
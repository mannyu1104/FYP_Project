using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// One full post row - used identically in the main feed and in the profile
/// page's post list. Shows the poster's icon/name (clickable -> their
/// profile), the post's content, and all of its comments, all at once -
/// there is no separate "detail" page to click into.
/// </summary>
public class SocialPostUI : MonoBehaviour
{
    [Header("Poster Header")]
    //[SerializeField] private CustomButtonUi posterIconButton; // click -> that poster's profile
    [SerializeField] private Image posterAvatarImage;
    [SerializeField] private TMP_Text posterNameText;

    [Header("Content")]
    [SerializeField] private TMP_Text contentText;

    [Header("Comments")]
    [SerializeField] private Transform commentContainer; // parent with a Vertical Layout Group
    [SerializeField] private SocialCommentItemUI commentItemPrefab;

    private readonly List<GameObject> spawnedComments = new List<GameObject>();

    public void Bind(SocialPostData post, SocialMediaPageController owner)
    {
        posterAvatarImage.sprite = post.Account.Avatar;
        posterNameText.text = post.Account.AccountName;
        contentText.text = post.Content;

        //posterIconButton.onLeftClick.RemoveAllListeners();
        //posterIconButton.onLeftClick.AddListener(() => owner.ShowProfile(post.Account));

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
            item.Bind(comment);
            spawnedComments.Add(item.gameObject);
        }
    }
}
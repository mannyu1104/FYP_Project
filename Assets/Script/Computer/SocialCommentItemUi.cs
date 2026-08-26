using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Comment list item prefab, one row in the list of comments. 
public class SocialCommentItemUI : MonoBehaviour
{
    [SerializeField] private CustomButtonUi commenterAvatarButton;
    [SerializeField] private CustomButtonUi commenterNameButton;
    [SerializeField] private Image commenterAvatarImage;
    [SerializeField] private TMP_Text commenterNameText;
    [SerializeField] private TMP_Text commentText;

    [Header("Name Color")]
    [SerializeField] private Color normalNameColor = Color.black;
    [SerializeField] private Color hasProfileNameColor;

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
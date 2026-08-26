using UnityEngine;
using TMPro;

// Comment list item prefab, one row in the list of comments. 
public class SocialCommentItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text commentText;

    public void Bind(SocialCommentEntry comment)
    {
        commentText.text = $"{comment.commenterName}: {comment.commentText}";
    }
}
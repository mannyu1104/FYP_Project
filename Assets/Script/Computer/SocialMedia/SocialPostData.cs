using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

// Social media post. Create instances via Assets > Create > Browser >
[CreateAssetMenu(fileName = "NewSocialPost", menuName = "ScriptableObject/Social Post")]
public class SocialPostData : ClueSourceData
{
    [Header("Post Details")]
    [SerializeField] private SocialAccountData account;
    //[SerializeField][TextArea(3, 10)] private string content;
    [SerializeField] private LocalizedString content;
    [SerializeField] private Sprite postImage; // Optional image for the post
    [SerializeField] private List<SocialCommentEntry> comments;

    public SocialAccountData Account => account;
    public LocalizedString Content => content;
    public Sprite PostImage => postImage;
    public IReadOnlyList<SocialCommentEntry> Comments => comments;
}
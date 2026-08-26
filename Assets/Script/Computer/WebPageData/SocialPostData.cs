using System.Collections.Generic;
using UnityEngine;

// Social media post. Create instances via Assets > Create > Browser >
[CreateAssetMenu(fileName = "NewSocialPost", menuName = "ScriptableObject/Social Post")]
public class SocialPostData : ScriptableObject
{
    [SerializeField] private SocialAccountData account;
    [SerializeField][TextArea(3, 10)] private string content;
    [SerializeField] private List<SocialCommentEntry> comments;

    public SocialAccountData Account => account;
    public string Content => content;
    public IReadOnlyList<SocialCommentEntry> Comments => comments;
}
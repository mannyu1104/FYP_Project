using System;
using UnityEngine;

// Comment on a social media post.
[Serializable]
public class SocialCommentEntry
{
    public SocialAccountData account;
    [TextArea(1, 4)] public string commentText;
    public bool hasProfilePage = false;
}
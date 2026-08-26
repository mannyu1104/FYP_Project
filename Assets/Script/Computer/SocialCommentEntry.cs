using System;
using UnityEngine;

// Comment on a social media post.
[Serializable]
public class SocialCommentEntry
{
    public string commenterName;
    [TextArea(1, 4)] public string commentText;
}
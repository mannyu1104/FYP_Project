using System;
using UnityEngine;

// Comment on a social media post.
[Serializable]
public class NewsCommentEntry
{
    public string name;
    [TextArea(1, 4)] public string commentText;
}
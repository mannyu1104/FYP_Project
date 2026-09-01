using System;
using UnityEngine;
using UnityEngine.Localization;

// Comment on a social media post.
[Serializable]
public class NewsCommentEntry
{
    public LocalizedString name;
    public LocalizedString commentText;
}
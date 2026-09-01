using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "NewNewsArticle", menuName = "ScriptableObject/News Article")]
public class NewsArticleData : ClueSourceData
{
    [Header("News Article Details")]
    [SerializeField] private LocalizedString headline;
    [SerializeField] private LocalizedString date;
    [SerializeField] private LocalizedString content;
    [SerializeField] private List<NewsCommentEntry> comments;

    public LocalizedString Headline => headline;
    public LocalizedString Date => date;
    public LocalizedString Content => content;
    public IReadOnlyList<NewsCommentEntry> Comments => comments;
}
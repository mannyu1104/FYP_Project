using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNewsArticle", menuName = "ScriptableObject/News Article")]
public class NewsArticleData : ClueSourceData
{
    [SerializeField] private string headline;
    [SerializeField] private string date;
    [SerializeField][TextArea(5, 20)] private string content;
    [SerializeField] private List<NewsCommentEntry> comments;

    public string Headline => headline;
    public string Date => date;
    public string Content => content;
    public IReadOnlyList<NewsCommentEntry> Comments => comments;
}
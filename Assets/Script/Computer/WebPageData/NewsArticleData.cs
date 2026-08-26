using UnityEngine;

[CreateAssetMenu(fileName = "NewNewsArticle", menuName = "ScriptableObject/News Article")]
public class NewsArticleData : ScriptableObject
{
    [SerializeField] private string headline;
    [SerializeField] private string date;
    [SerializeField][TextArea(5, 20)] private string content;

    public string Headline => headline;
    public string Date => date;
    public string Content => content;
}
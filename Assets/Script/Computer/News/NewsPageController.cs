using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Manages navigation within news pages
public class NewsPageController : ListDetailPageController<NewsArticleData, NewsListItemUI>
{
    [Header("Detail View Fields")]
    [SerializeField] private TMP_InputField detailHeadlineText;
    [SerializeField] private TMP_InputField detailDateText;
    [SerializeField] private TMP_InputField detailContentText;

    [Header("Comments")]
    [SerializeField] private Transform commentContainer;
    [SerializeField] private NewsCommentItemUi commentItemPrefab;
    [SerializeField] private ScrollRect commentContainerRect;

    [Header("Clue")]
    [SerializeField] private ClueRecordButton clueRecordButton;

    public void ShowArticleDetail(NewsArticleData article) => ShowDetail(article);

    private readonly List<GameObject> spawnedComments = new List<GameObject>();

    protected override void PopulateDetail(NewsArticleData article)
    {
        detailHeadlineText.text = article.Headline;
        detailDateText.text = article.Date;
        detailContentText.text = article.Content;
        detailContentText.verticalScrollbar.value = 0f; // Scroll to top

        BuildComments(article);

        commentContainerRect.verticalNormalizedPosition = 1f; // Scroll to top
    }

    protected override void OnDetailShown(NewsArticleData article)
    {
        clueRecordButton.SetSource(article);
    }

    protected override void BindListItem(NewsListItemUI listItemUI, NewsArticleData article)
    {
        listItemUI.Bind(article, this);
    }

    private void BuildComments(NewsArticleData article)
    {
        foreach (GameObject item in spawnedComments)
            Destroy(item);
        spawnedComments.Clear();

        foreach (NewsCommentEntry comment in article.Comments)
        {
            NewsCommentItemUi item = Instantiate(commentItemPrefab, commentContainer);
            item.Bind(comment, this);
            spawnedComments.Add(item.gameObject);
        }
    }
}
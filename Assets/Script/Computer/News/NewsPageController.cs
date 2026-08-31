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

    private NewsArticleData articleData;

    protected override void PopulateDetail(NewsArticleData article)
    {
        UnsubscribeFromLocalization();
        articleData = article;

        SubscribeToLocalization();

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

    private void SubscribeToLocalization()
    {
        if (articleData == null) return;
        articleData.Headline.StringChanged += UpdateHeadlineText;
        articleData.Date.StringChanged += UpdateDateText;
        articleData.Content.StringChanged += UpdateContentText;
    }

    private void UnsubscribeFromLocalization()
    {
        if (articleData == null) return;
        articleData.Headline.StringChanged -= UpdateHeadlineText;
        articleData.Date.StringChanged -= UpdateDateText;
        articleData.Content.StringChanged -= UpdateContentText;
    }

    private void UpdateHeadlineText(string value)
    {
        if (detailHeadlineText != null) detailHeadlineText.text = value;
    }

    private void UpdateDateText(string value)
    {
        if (detailDateText != null) detailDateText.text = value;
    }

    private void UpdateContentText(string value)
    {
        if (detailContentText != null) detailContentText.text = value;
    }

    private void OnDestroy()
    {
        UnsubscribeFromLocalization();
    }
}
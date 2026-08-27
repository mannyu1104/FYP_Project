using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

// Manages navigation within news pages
public class NewsPageController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private List<NewsArticleData> articles;

    [Header("List View")]
    [SerializeField] private CanvasGroup listCanvasGroup;
    [SerializeField] private Transform listContainer;
    [SerializeField] private NewsListItemUI listItemPrefab;

    [Header("Detail View")]
    [SerializeField] private CanvasGroup detailCanvasGroup;
    [SerializeField] private Transform detailTextContainer;
    [SerializeField] private TMP_InputField detailHeadlineText;
    [SerializeField] private TMP_InputField detailDateText;
    [SerializeField] private TMP_InputField detailContentText;
    //[SerializeField] private ScrollRect detailContentScrollRect;
    [SerializeField] private CustomButtonUi backButton;

    [Header("Clue")]
    [SerializeField] private ClueRecordButton clueRecordButton;

    private void Awake()
    {
        backButton.onLeftClick.AddListener(ShowList);
        BuildListOnce();

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(listContainer.GetComponent<RectTransform>());
        
        Show(listCanvasGroup);
        Hide(detailCanvasGroup);
    }

    private void OnEnable()
    {
        // Start from list view every time the player opens the News tab
        ShowList();
    }

    public void ShowList()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(listContainer.GetComponent<RectTransform>());

        Show(listCanvasGroup);
        Hide(detailCanvasGroup);
    }

    public void ShowArticleDetail(NewsArticleData article)
    {

        detailHeadlineText.text = article.Headline;
        detailDateText.text = article.Date;
        detailContentText.text = article.Content;

        LayoutRebuilder.ForceRebuildLayoutImmediate(detailTextContainer.GetComponent<RectTransform>());
        


        Show(detailCanvasGroup);
        Hide(listCanvasGroup);

        clueRecordButton.SetSource(article);
    }

    private void Show(CanvasGroup group)
    {
        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    private void Hide(CanvasGroup group)
    {
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    private void BuildListOnce()
    {
        foreach (NewsArticleData article in articles)
        {
            NewsListItemUI item = Instantiate(listItemPrefab, listContainer);
            item.Bind(article, this);
        }
    }

}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Base class for the "List -> Detail" navigation pattern. Examples: NewsPageController, OrphanageHPController
public abstract class ListDetailPageController<TItem, TListItemUI> : MonoBehaviour
    where TListItemUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] protected List<TItem> items;

    [Header("List View")]
    [SerializeField] protected CanvasGroup listCanvasGroup;
    [SerializeField] protected Transform listContainer;
    [SerializeField] protected TListItemUI listItemPrefab;

    [Header("Detail View")]
    [SerializeField] protected CanvasGroup detailCanvasGroup;
    [SerializeField] protected Transform detailTextContainer;
    [SerializeField] protected CustomButtonUi backButton;

    protected virtual void Awake()
    {
        backButton.onLeftClick.AddListener(ShowList);
        BuildListOnce();

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(listContainer.GetComponent<RectTransform>());

        Show(listCanvasGroup);
        Hide(detailCanvasGroup);
    }

    protected virtual void OnEnable()
    {
        // Start from list view every time the player opens this tab.
        ShowList();
    }

    public virtual void ShowList()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(listContainer.GetComponent<RectTransform>());

        Show(listCanvasGroup);
        Hide(detailCanvasGroup);
    }

    // Shows the detail view for the given item.
    protected void ShowDetail(TItem item)
    {
        PopulateDetail(item);

        LayoutRebuilder.ForceRebuildLayoutImmediate(detailTextContainer.GetComponent<RectTransform>());

        Show(detailCanvasGroup);
        Hide(listCanvasGroup);

        OnDetailShown(item);
    }

    // Fill in the detail view's fields from the given item.
    protected abstract void PopulateDetail(TItem item);

    protected virtual void OnDetailShown(TItem item) { }

    // Bind one list item's UI to its data. 
    protected abstract void BindListItem(TListItemUI listItemUI, TItem item);

    protected void Show(CanvasGroup group)
    {
        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    protected void Hide(CanvasGroup group)
    {
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    private void BuildListOnce()
    {
        foreach (TItem item in items)
        {
            TListItemUI listItemUI = Instantiate(listItemPrefab, listContainer);
            BindListItem(listItemUI, item);
        }
    }
}
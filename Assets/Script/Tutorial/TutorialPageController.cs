using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

// Manages navigation between the witness list and a witness's detail view.
// Structurally identical to NewsPageController - same list/detail CanvasGroup
// switching, same "bind a shared ClueRecordButton to whichever item is open" pattern.
public class TutorialPageController : MonoBehaviour
{

    [Header("List View")]
    //[SerializeField] private CanvasGroup listCanvasGroup;
    [SerializeField] private Transform clueContainer;
    //[SerializeField] private TutorialClueItemUi listItemPrefab;

    [Header("Detail View")]
    [SerializeField] private CanvasGroup detailCanvasGroup;
    [SerializeField] private Transform detailTextContainer;
    [SerializeField] private TMP_InputField detailNameText;
    [SerializeField] private TMP_InputField detailDescriptionText;
    [SerializeField] private Image detailPhotoImage; // hidden automatically if the witness has no photo
    [SerializeField] private CustomButtonUi backButton;

    [Header("Clue")]
    [SerializeField] private ClueRecordButton clueRecordButton;

    private TutorialClueData clueData;

    private void Awake()
    {
        backButton.onLeftClick.AddListener(ShowList);
        BindItems();

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(clueContainer.GetComponent<RectTransform>());

        //Show(listCanvasGroup);
        Hide(detailCanvasGroup);
    }

    private void OnEnable()
    {
        // Start from the witness list every time this panel is opened
        ShowList();
    }

    public void ShowList()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(clueContainer.GetComponent<RectTransform>());

        //Show(listCanvasGroup);
        Hide(detailCanvasGroup);
    }

    public void ShowTutorialClueDetail(TutorialClueData tutorialClue)
    {
        UnsubscribeFromLocalization();
        clueData = tutorialClue;

        SubscribeToLocalization();

        bool hasPhoto = tutorialClue.ClueImage != null;
        detailPhotoImage.gameObject.SetActive(hasPhoto);
        if (hasPhoto)
        {
            detailPhotoImage.sprite = tutorialClue.ClueImage;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(detailTextContainer.GetComponent<RectTransform>());

        Show(detailCanvasGroup);
        //Hide(listCanvasGroup);

        clueRecordButton.SetSource(tutorialClue);
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

    private void BindItems()
    {
        // true = include inactive, in case some items start disabled/hidden.
        TutorialClueItemUi[] items = clueContainer.GetComponentsInChildren<TutorialClueItemUi>(true);
        foreach (TutorialClueItemUi item in items)
        {
            item.Bind(this);
        }
    }

    private void SubscribeToLocalization()
    {
        if (clueData == null) return;
        clueData.TutorialClueName.StringChanged += UpdateNameText;
        clueData.TutorialClueDescription.StringChanged += UpdateDescriptionText;
    }

    private void UnsubscribeFromLocalization()
    {
        if (clueData == null) return;
        clueData.TutorialClueName.StringChanged -= UpdateNameText;
        clueData.TutorialClueDescription.StringChanged -= UpdateDescriptionText;
    }

    private void UpdateNameText(string value)
    {
        if (detailNameText != null) detailNameText.text = value;
    }

    private void UpdateDescriptionText(string value)
    {
        if (detailDescriptionText != null) detailDescriptionText.text = value;
    }

    private void OnDestroy()
    {
        UnsubscribeFromLocalization();
    }
}
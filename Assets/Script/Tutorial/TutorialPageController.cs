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
    [SerializeField] private Image detailAvatarImage;
    [SerializeField] private GameObject detailImageContainer;
    [SerializeField] private Image detailImage;
    [SerializeField] private CustomButtonUi backButton;

    [Header("Clue")]
    [SerializeField] private ClueRecordButton clueRecordButton;

    [SerializeField] private GameObject submissionPanel;
    private readonly Dictionary<GameObject, bool> hiddenSubmissionPanels = new Dictionary<GameObject, bool>();
    private TutorialClueData clueData;

    private void Awake()
    {
        IntegrateSubmissionPanel();
        backButton.onLeftClick.AddListener(ShowList);
        BindItems();

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(clueContainer.GetComponent<RectTransform>());

        //Show(listCanvasGroup);
        Hide(detailCanvasGroup);
    }

    private bool submissionIntegrated;
    public void IntegrateSubmissionPanel()
    {
        if (submissionIntegrated || submissionPanel == null) return;
        submissionIntegrated = true;
        var destination = transform;
        var candidates = new List<GameObject>();
        var canonical = submissionPanel;
        foreach (Transform candidate in FindObjectsByType<Transform>(FindObjectsInactive.Include))
        {
            if (candidate.name == "SubmissionPage" && candidate.gameObject.scene == gameObject.scene)
                candidates.Add(candidate.gameObject);
        }
        // Use the explicitly assigned computer panel. Slot counts cannot identify it:
        // the merged panels keep their inventory slots elsewhere.
        canonical.transform.SetParent(destination, false);
        var rect = (RectTransform)canonical.transform;
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, .5f);
        rect.sizeDelta = new Vector2(650f, -130f);
        rect.anchoredPosition = new Vector2(-50f, -15f);
        rect.localScale = Vector3.one;
        var layout = canonical.GetComponent<LayoutElement>();
        if (layout != null) layout.ignoreLayout = true;
        foreach (var candidate in candidates) if (candidate != canonical) candidate.SetActive(false);
        submissionPanel = canonical;
        submissionPanel.SetActive(true);
        SetOverlayOrder(submissionPanel, 100);
        submissionPanel.transform.SetAsLastSibling();
        foreach (var button in canonical.GetComponentsInChildren<Button>(true))
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                if (button.onClick.GetPersistentTarget(i) is GameObject score && score.name == "ScoreBack")
                {
                    score.transform.SetParent(destination, false);
                    SetOverlayOrder(score, 2100);
                    score.SetActive(false);
                }
    }

    private static void SetOverlayOrder(GameObject panel, int order)
    {
        var canvas = panel.GetComponent<Canvas>();
        if (canvas == null) canvas = panel.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = order;
        if (panel.GetComponent<GraphicRaycaster>() == null) panel.AddComponent<GraphicRaycaster>();
    }

    private void OnEnable()
    {
        // Start from the witness list every time this panel is opened
        ShowList();
    }

    public void ShowList()
    {
        RestoreSubmissionPanel();
        if (submissionPanel != null) submissionPanel.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(clueContainer.GetComponent<RectTransform>());

        //Show(listCanvasGroup);
        Hide(detailCanvasGroup);
    }

    public void ShowTutorialClueDetail(TutorialClueData tutorialClue)
    {
        UnsubscribeFromLocalization();
        clueData = tutorialClue;

        SubscribeToLocalization();

        bool hasPhoto = tutorialClue.ClueAvatar != null;
        detailAvatarImage.gameObject.SetActive(hasPhoto);
        if (hasPhoto)
        {
            detailAvatarImage.sprite = tutorialClue.ClueAvatar;
        }

        bool hasImage = tutorialClue.ClueImage != null;
        detailImageContainer.SetActive(hasImage);
        if (hasImage)
        {
            detailImage.sprite = tutorialClue.ClueImage;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(detailTextContainer.GetComponent<RectTransform>());

        if (submissionPanel != null && !hiddenSubmissionPanels.ContainsKey(submissionPanel))
        {
            hiddenSubmissionPanels.Add(submissionPanel, submissionPanel.activeSelf);
            submissionPanel.SetActive(false);
        }
        detailCanvasGroup.transform.SetAsLastSibling();
        Show(detailCanvasGroup);
        //Hide(listCanvasGroup);

        clueRecordButton.SetSource(tutorialClue);
    }

    private void OnDisable() => RestoreSubmissionPanel();

    private void RestoreSubmissionPanel()
    {
        foreach (var pair in hiddenSubmissionPanels)
            if (pair.Key != null) pair.Key.SetActive(pair.Value);
        hiddenSubmissionPanels.Clear();
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
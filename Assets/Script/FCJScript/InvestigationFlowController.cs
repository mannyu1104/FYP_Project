using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Owns the opening sequence and the visibility of the persistent investigation controls.
[DefaultExecutionOrder(1000)]
public class InvestigationFlowController : MonoBehaviour
{
    public enum Stage { Menu, Introduction, AwaitTutorial, Tutorial, AfterTutorial, Sleeping,
        Morning, AwaitNews, ReadingNews, AfterNews, AwaitDoor, ClientConversation, Investigation }
    public static InvestigationFlowController Instance { get; private set; }
    public Stage CurrentStage { get; private set; } = Stage.Menu;
    public bool IsOpeningSequence => CurrentStage != Stage.Menu && CurrentStage != Stage.Investigation;
    public bool CanOpenComputer => menu != null && !menu.IsSettingsVisible && !dialogue.IsDialogueActive &&
        (CurrentStage == Stage.AwaitTutorial || CurrentStage == Stage.Tutorial ||
         CurrentStage == Stage.AwaitNews || CurrentStage == Stage.ReadingNews || CurrentStage == Stage.Investigation);
    public bool IsGuidedComputer => CurrentStage == Stage.Tutorial || CurrentStage == Stage.ReadingNews;

    private MainMenuController menu;
    private DialogueController dialogue;
    private ComputerCanvasController computer;
    private NPCDialogueTrigger client;
    private GameObject home, door, tutorial, internet;
    private Button doorButton;
    private bool doorButtonEnabled;
    private NewsArticleData openingArticle;
    private bool articleWasOpened;
    public bool TutorialScored { get; set; }
    private readonly List<GameObject> dragHints = new List<GameObject>();
    private bool dragHintDismissed, dragHintWasVisible;
    private Coroutine sequence;
    public int ConversationPart { get; private set; }
    private DialogueController.ConversationSnapshot resumeConversation;
    private Image sleepOverlay;
    private readonly Dictionary<GameObject, bool> worldObjects = new Dictionary<GameObject, bool>();
    private readonly Dictionary<GameObject, bool> computerObjects = new Dictionary<GameObject, bool>();
    private readonly List<GameObject> sideButtons = new List<GameObject>();
    private readonly List<GameObject> notebookButtons = new List<GameObject>();
    private readonly List<GameObject> tutorialLaunchers = new List<GameObject>();
    private readonly List<GameObject> popupRoots = new List<GameObject>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
        SceneManager.sceneLoaded += SceneLoaded;
        EnsureInstalled();
    }
    private static void SceneLoaded(Scene scene, LoadSceneMode mode) => EnsureInstalled();
    private static void EnsureInstalled()
    {
        if (FindAnyObjectByType<MainMenuController>(FindObjectsInactive.Include) == null ||
            FindAnyObjectByType<InvestigationFlowController>() != null) return;
        new GameObject("InvestigationFlow").AddComponent<InvestigationFlowController>();
    }

    private void Awake()
    {
        Instance = this;
        menu = FindAnyObjectByType<MainMenuController>(FindObjectsInactive.Include);
        dialogue = FindAnyObjectByType<DialogueController>(FindObjectsInactive.Include);
        computer = FindAnyObjectByType<ComputerCanvasController>(FindObjectsInactive.Include);
        home = FindObject("Home\u5bb6Panel");
        tutorial = FindObject("Tutorial Panel");
        if (tutorial != null)
            foreach (var page in tutorial.GetComponentsInChildren<TutorialPageController>(true)) page.IntegrateSubmissionPanel();
        internet = FindObject("Internet  Discovery Panel");
        foreach (ForVideoSceneFlowController oldFlow in FindObjectsByType<ForVideoSceneFlowController>(FindObjectsInactive.Include))
            oldFlow.enabled = false;
        foreach (CursorInteractionTarget target in FindObjectsByType<CursorInteractionTarget>(FindObjectsInactive.Include))
        {
            worldObjects[target.gameObject] = target.gameObject.activeSelf;
            if (home != null && target.transform.IsChildOf(home.transform) && target.name.Contains("\u51fa\u95e8"))
                door = target.gameObject;
        }
        foreach (NPCDialogueTrigger npc in FindObjectsByType<NPCDialogueTrigger>(FindObjectsInactive.Include))
            if (npc.name.Contains("\u59d4\u6258\u4eba")) { client = npc; break; }
        if (door != null)
        {
            doorButton = door.GetComponent<Button>();
            doorButtonEnabled = doorButton != null && doorButton.enabled;
        }
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Include))
        {
            if (t.name == "DeleteSaveTestButton" || t.name == "TestingPanel" || t.name == "SettingIcon") t.gameObject.SetActive(false);
            if (t.name == "NoteBookButton")
            {
                notebookButtons.Add(t.gameObject);
                t.gameObject.SetActive(false);
            }
            if (!t.name.StartsWith("NoteButton(")) continue;
            sideButtons.Add(t.gameObject);
            Canvas shortcutCanvas = t.GetComponent<Canvas>();
            if (shortcutCanvas == null) shortcutCanvas = t.gameObject.AddComponent<Canvas>();
            shortcutCanvas.overrideSorting = true;
            shortcutCanvas.sortingOrder = 2000; // The page covers the inner edge of each tab.
            if (t.GetComponent<GraphicRaycaster>() == null) t.gameObject.AddComponent<GraphicRaycaster>();
            RectTransform rect = t as RectTransform;
            if (rect != null)
            {
                // Preserve the authored size, spacing and functional order. Object discovery
                // order is unspecified, so it must never determine button positions.
                rect.anchoredPosition += new Vector2(-24f, 0f);
            }
            Button button = t.GetComponent<Button>();
            if (button != null)
                for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                    if (button.onClick.GetPersistentTarget(i) is GameObject panel && panel != t.gameObject)
                        if (!popupRoots.Contains(panel)) popupRoots.Add(panel);
            t.gameObject.SetActive(false);
        }
        // Item details replace the inventory panel, so track them independently too.
        foreach (DragableItem item in FindObjectsByType<DragableItem>(FindObjectsInactive.Include))
            if (item.DesUI != null && !popupRoots.Contains(item.DesUI)) popupRoots.Add(item.DesUI);
        foreach (GameObject popup in popupRoots)
        {
            Canvas popupCanvas = popup.GetComponent<Canvas>();
            if (popupCanvas == null) popupCanvas = popup.AddComponent<Canvas>();
            popupCanvas.overrideSorting = true;
            popupCanvas.sortingOrder = 2001;
            if (popup.GetComponent<GraphicRaycaster>() == null) popup.AddComponent<GraphicRaycaster>();
            popup.SetActive(false);
        }
        SetObject(FindObject("MapPanel"), false);
        SetObject(FindObject("CanvasMap"), false);
        SetObject(FindObject("WhiteBoardPanel"), false);
        if (tutorial != null)
            foreach (Transform child in tutorial.transform.parent) computerObjects[child.gameObject] = child.gameObject.activeSelf;
        foreach (CustomButtonUi app in FindObjectsByType<CustomButtonUi>(FindObjectsInactive.Include))
            if (app.name == "Tutorial App" || TargetsTutorial(app.onDoubleClick) || TargetsTutorial(app.onLeftClick))
            {
                tutorialLaunchers.Add(app.gameObject);
                app.onLeftClick.AddListener(() => { if (CurrentStage == Stage.Tutorial) ShowPanel(tutorial); });
            }
        GameObject legacyMap = FindObject("CanvasMap");
        if (legacyMap != null) popupRoots.Add(legacyMap);
        ClueSubmission.AssessmentScored += TutorialFinished;
        foreach (var panels in FindObjectsByType<OpenCanvasButton>(FindObjectsInactive.Include)) panels.CloseAll();
        CountingPoint.ScoreShown += LegacyTutorialScored;
        ApplyWorldVisibility();
        if (GetComponent<WelfareInteractionController>() == null) gameObject.AddComponent<WelfareInteractionController>();
    }

    private void OnDestroy()
    {
        ClueSubmission.AssessmentScored -= TutorialFinished;
        CountingPoint.ScoreShown -= LegacyTutorialScored;
        if (Instance == this) Instance = null;
    }

    public void BeginNewGame()
    {
        StopSequence();
        if (home != null) FindAnyObjectByType<MapPanelNavigator>()?.ResetToHome();
        TutorialScored = false;
        dragHintDismissed = false;
        dragHintWasVisible = false;
        CurrentStage = Stage.Introduction;
        ApplyWorldVisibility();
        sequence = StartCoroutine(Introduction());
    }

    private IEnumerator Introduction()
    {
        yield return Speak("Story_002", "Story_003", "Story_004", "Story_005");
        SetStage(Stage.AwaitTutorial);
    }

    private IEnumerator Speak(params string[] keys)
    {
        yield return LocalizationSettings.InitializationOperation;
        while (menu.IsSettingsVisible) yield return null;
        var lines = new List<DialogueController.DialogueLine>();
        foreach (string key in keys) lines.Add(new DialogueController.DialogueLine
            { speakerName = key.StartsWith("Story_") ? new LocalizedString("Script Assets", "Character_010") : null,
              dialogueText = new LocalizedString("Script Assets", key) });
        if (resumeConversation != null)
        { dialogue.RestoreConversation(resumeConversation); resumeConversation = null; }
        else dialogue.StartConversation(lines, null);
        while (dialogue.IsDialogueActive) yield return null;
    }

    public void OnComputerOpened()
    {
        if (!IsOpeningSequence) return;
        articleWasOpened = false;
        SetStage(CurrentStage == Stage.AwaitTutorial || CurrentStage == Stage.Tutorial ? Stage.Tutorial : Stage.ReadingNews);
        StartCoroutine(RouteComputer());
    }

    private IEnumerator RouteComputer()
    {
        if (CurrentStage == Stage.Tutorial) yield return null;
        if (CurrentStage == Stage.Tutorial)
        {
            // Enter the desktop first. The tutorial launcher opens the lesson on demand.
            foreach (var pair in computerObjects) SetObject(pair.Key, pair.Value);
            SetObject(tutorial, false);
            SetObject(internet, false);
            foreach (CustomButtonUi app in FindObjectsByType<CustomButtonUi>(FindObjectsInactive.Include))
            {
                if (tutorial == null || app.transform.IsChildOf(tutorial.transform) ||
                    (internet != null && app.transform.IsChildOf(internet.transform)) ||
                    !app.transform.IsChildOf(tutorial.transform.parent)) continue;
                if (!computerObjects.ContainsKey(app.gameObject)) computerObjects.Add(app.gameObject, app.gameObject.activeSelf);
                SetObject(app.gameObject, tutorialLaunchers.Contains(app.gameObject));
            }
            foreach (GameObject app in tutorialLaunchers) SetObject(app, true);
            yield break;
        }
        GameObject selected = internet;
        foreach (var pair in computerObjects) SetObject(pair.Key, pair.Key == selected);
        ShowPanel(selected);
        if (CurrentStage != Stage.ReadingNews) yield break;
        NewsPageController news = FindAnyObjectByType<NewsPageController>(FindObjectsInactive.Include);
        if (news == null) yield break;
        foreach (NewsArticleData article in news.Articles)
            if (article != null && article.name.Contains("\u706b\u707e\uff0c\u9662\u957f")) { openingArticle = article; break; }
        if (openingArticle == null)
        {
            Debug.LogError("Opening fire/missing-director article is not configured.", this);
            yield break;
        }
        foreach (BrowserAppButton app in FindObjectsByType<BrowserAppButton>(FindObjectsInactive.Include))
            RememberAndHide(app.gameObject);
        foreach (SearchBarController search in FindObjectsByType<SearchBarController>(FindObjectsInactive.Include))
            RememberAndHide(search.gameObject);
        BrowserTabManager.Instance.ResetActiveTab();
        WebPageContentController.Instance.OpenNewsArticle(openingArticle);
    }

    private void RememberAndHide(GameObject obj)
    {
        if (!computerObjects.ContainsKey(obj)) computerObjects.Add(obj, obj.activeSelf);
        obj.SetActive(false);
    }

    public bool AllowsArticle(NewsArticleData article) => !IsOpeningSequence ||
        (CurrentStage == Stage.ReadingNews && article == openingArticle);

    public void ArticleOpened(NewsArticleData article)
    {
        if (CurrentStage == Stage.ReadingNews && article == openingArticle) articleWasOpened = true;
    }

    public void ArticleClosed()
    {
        if (CurrentStage != Stage.ReadingNews || !articleWasOpened) return;
        SetStage(Stage.AfterNews);
        computer.CloseComputer();
        sequence = StartCoroutine(AfterNews());
    }

    public void OnComputerClosed()
    {
        if (CurrentStage == Stage.ReadingNews) ArticleClosed();
        else if (CurrentStage == Stage.Tutorial)
        {
            if (TutorialScored)
            {
                SetStage(Stage.AfterTutorial);
                sequence = StartCoroutine(EndFirstDay());
            }
            else SetStage(Stage.AwaitTutorial);
        }
    }

    private void TutorialFinished(ClueSubmission assessment)
    {
        if (CurrentStage != Stage.Tutorial || tutorial == null || assessment.Board == null ||
            !assessment.Board.transform.IsChildOf(tutorial.transform) || !assessment.HasScoredSubmission) return;
        TutorialScored = true;
    }

    private void LegacyTutorialScored()
    {
        if (CurrentStage == Stage.Tutorial) TutorialScored = true;
    }

    private IEnumerator EndFirstDay()
    {
        if (CurrentStage == Stage.AfterTutorial) yield return Speak("Story_007");
        SetStage(Stage.Sleeping);
        CreateSleepOverlay();
        yield return FadeSleep(0, 1);
        yield return new WaitForSecondsRealtime(1.5f);
        SetStage(Stage.Morning);
        yield return FadeSleep(1, 0);
        sleepOverlay.gameObject.SetActive(false);
        yield return Speak("Story_008", "Story_009");
        SetStage(Stage.AwaitNews);
    }

    private IEnumerator AfterNews()
    {
        yield return Speak("Story_011");
        PlayKnock();
        SetStage(Stage.AwaitDoor);
    }

    public bool TryOpenDoor(GameObject target)
    {
        if (target != door || CurrentStage != Stage.AwaitDoor || menu.IsSettingsVisible || dialogue.IsDialogueActive) return false;
        SetStage(Stage.ClientConversation);
        sequence = StartCoroutine(MeetClient());
        return true;
    }

    private IEnumerator MeetClient()
    {
        if (ConversationPart == 0) yield return Speak("Story_012");
        if (ConversationPart < 2)
        {
        ConversationPart = 1;
        if (client != null)
        {
            client.gameObject.SetActive(true);
            if (resumeConversation != null)
            { dialogue.RestoreConversation(resumeConversation); resumeConversation = null; }
            else { client.ResetProgress(); client.StartDialogue(); }
            while (dialogue.IsDialogueActive) yield return null;
        }
        else yield return Speak("Dialogue_001", "Dialogue_002", "Dialogue_003");
        }
        ConversationPart = 2;
        yield return Speak("Story_013");
        SetStage(Stage.Investigation);
    }

    private void SetStage(Stage stage)
    {
        CurrentStage = stage;
        if (stage == Stage.Sleeping || stage == Stage.AwaitNews) PreloadOpeningNews();
        ApplyWorldVisibility();
        if (stage == Stage.Investigation)
        {
            foreach (var pair in computerObjects) SetObject(pair.Key, pair.Value);
            SetObject(tutorial, false);
            SetObject(internet, false);
            foreach (GameObject app in tutorialLaunchers) SetObject(app, false);
        }
    }

    private void PreloadOpeningNews()
    {
        var news = FindAnyObjectByType<NewsPageController>(FindObjectsInactive.Include);
        if (news == null) return;
        foreach (var article in news.Articles)
            if (article != null && article.name.Contains("\u706b\u707e\uff0c\u9662\u957f"))
            {
                openingArticle = article;
                article.Headline.GetLocalizedStringAsync();
                article.Date.GetLocalizedStringAsync();
                article.Content.GetLocalizedStringAsync();
                break;
            }
    }

    private bool TargetsTutorial(UnityEngine.Events.UnityEvent action)
    {
        if (action == null || tutorial == null) return false;
        for (int i = 0; i < action.GetPersistentEventCount(); i++)
            if (action.GetPersistentTarget(i) == tutorial) return true;
        return false;
    }

    private void ApplyWorldVisibility()
    {
        foreach (var pair in worldObjects)
        {
            if (pair.Key == null) continue;
            bool isComputer = pair.Key.GetComponent<ComputerAccessPoint>() != null;
            bool isSettings = pair.Key.name == "SettingButton(From InGame)";
            bool visible = CurrentStage == Stage.Investigation ? pair.Value :
                CurrentStage != Stage.Menu && (isComputer || isSettings || (pair.Key == door && CurrentStage == Stage.AwaitDoor));
            SetObject(pair.Key, visible);
        }
        if (doorButton != null) doorButton.enabled = CurrentStage == Stage.Investigation && doorButtonEnabled;
        if (client != null) SetObject(client.gameObject, CurrentStage == Stage.ClientConversation);
    }

    private void LateUpdate()
    {
        if (menu == null || dialogue == null) return;
        if (dragHints.Count == 0)
            foreach (var rect in FindObjectsByType<RectTransform>(FindObjectsInactive.Include))
                if (rect.name == "Drag Tutorial Hint" || rect.name == "Drag Direction" || rect.name.StartsWith("Arrow Shaft") || rect.name.StartsWith("Arrow Head"))
                    dragHints.Add(rect.gameObject);
        bool hintVisible = !dragHintDismissed && CurrentStage == Stage.Tutorial && tutorial != null && tutorial.activeInHierarchy;
        bool notebookVisible = false;
        foreach (var panel in FindObjectsByType<OpenCanvasButton>(FindObjectsInactive.Include)) notebookVisible |= panel.IsNotebookOpen;
        hintVisible &= notebookVisible;
        if (hintVisible && dragHintWasVisible && Input.GetMouseButtonDown(0)) { dragHintDismissed = true; hintVisible = false; }
        foreach (var hint in dragHints) if (hint != null) hint.SetActive(hintVisible);
        dragHintWasVisible = hintVisible;
        bool modal = WelfareInteractionController.IsOpen || SaveSlotPanel.IsOpen || menu.IsMenuVisible || menu.IsSettingsVisible || dialogue.IsDialogueActive || dialogue.IsHistoryOpen || MapButton.IsAnyMapOpen;
        foreach (GameObject popup in popupRoots) modal |= IsVisible(popup);
        var inventoryPanels = FindObjectsByType<OpenCanvasButton>(FindObjectsInactive.Include);
        foreach (var inventory in inventoryPanels) modal |= inventory.IsAnyOpen;
        bool itemDetailsVisible = false;
        foreach (var item in FindObjectsByType<DragableItem>(FindObjectsInactive.Include))
            if (IsVisible(item.DesUI)) { itemDetailsVisible = true; break; }
        bool show = CurrentStage != Stage.Menu &&
            (menu.IsGameplayVisible || computer.IsComputerOpen()) && !modal;
        foreach (GameObject button in notebookButtons) SetObject(button, show);
        foreach (GameObject button in sideButtons)
        {
            bool visible = show;
            foreach (var inventory in inventoryPanels)
                if (inventory.OwnsShortcut(button))
                {
                    visible = inventory.IsNotebookOpen && !menu.IsMenuVisible && !menu.IsSettingsVisible &&
                        !dialogue.IsDialogueActive && !dialogue.IsHistoryOpen && !SaveSlotPanel.IsOpen && !WelfareInteractionController.IsOpen;
                    visible &= !itemDetailsVisible;
                    break;
                }
            SetObject(button, visible);
        }
        if (client != null && CurrentStage != Stage.ClientConversation) SetObject(client.gameObject, false);
        if (CurrentStage == Stage.ReadingNews && articleWasOpened && computer.IsComputerOpen() && !IsVisible(internet)) ArticleClosed();

    }

    public void StopForMenu()
    {
        StopSequence();
        foreach (var inventory in FindObjectsByType<OpenCanvasButton>(FindObjectsInactive.Include)) inventory.CloseAll();
        WelfareInteractionController.Instance?.StopForMenu();
        dialogue.CancelConversation();
        CurrentStage = Stage.Menu;
        computer.CloseComputer();
        if (sleepOverlay != null) sleepOverlay.gameObject.SetActive(false);
        foreach (GameObject popup in popupRoots) SetObject(popup, false);
        ApplyWorldVisibility();
    }
    private void StopSequence()
    {
        StopAllCoroutines();
        sequence = null;
    }

    [Serializable] private class Progress { public Stage stage; }
    public void SaveProgress()
    {
        if (CurrentStage == Stage.Menu || MainMenuController.IsStartingNewGame) return;
        Stage resume = CurrentStage;
        if (resume == Stage.Introduction || resume == Stage.Tutorial) resume = Stage.AwaitTutorial;
        if (resume == Stage.AfterTutorial || resume == Stage.Sleeping || resume == Stage.Morning || resume == Stage.ReadingNews) resume = Stage.AwaitNews;
        if (resume == Stage.AfterNews || resume == Stage.ClientConversation) resume = Stage.AwaitDoor;
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "story_progress.json"), JsonUtility.ToJson(new Progress { stage = resume }));
    }
    public void LoadProgress()
    {
        StopSequence();
        string path = Path.Combine(Application.persistentDataPath, "story_progress.json");
        Stage stage = File.Exists(path) ? JsonUtility.FromJson<Progress>(File.ReadAllText(path)).stage : Stage.Investigation;
        articleWasOpened = false;
        if (!Enum.IsDefined(typeof(Stage), stage) || stage == Stage.Menu) stage = Stage.AwaitTutorial;
        SetStage(stage);
        if (stage == Stage.AwaitDoor) PlayKnock();
    }

    public void RestoreStage(Stage stage, int part, DialogueController.ConversationSnapshot conversation)
    {
        StopSequence();
        ConversationPart = part;
        resumeConversation = conversation;
        SetStage(stage);
        switch (stage)
        {
            case Stage.Introduction: sequence = StartCoroutine(Introduction()); break;
            case Stage.AfterTutorial:
            case Stage.Sleeping: sequence = StartCoroutine(EndFirstDay()); break;
            case Stage.Morning: sequence = StartCoroutine(ResumeMorning()); break;
            case Stage.AfterNews: sequence = StartCoroutine(AfterNews()); break;
            case Stage.ClientConversation: sequence = StartCoroutine(MeetClient()); break;
            case Stage.Tutorial:
            case Stage.ReadingNews: computer.OpenComputer(); break;
            default:
                if (conversation != null) dialogue.RestoreConversation(conversation);
                resumeConversation = null;
                break;
        }
    }
    private IEnumerator ResumeMorning()
    {
        yield return Speak("Story_008", "Story_009");
        SetStage(Stage.AwaitNews);
    }

    private void CreateSleepOverlay()
    {
        if (sleepOverlay != null) { sleepOverlay.gameObject.SetActive(true); return; }
        GameObject root = new GameObject("Sleep Transition", typeof(Canvas), typeof(GraphicRaycaster));
        root.transform.SetParent(transform);
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30000;
        GameObject shade = new GameObject("Black", typeof(RectTransform), typeof(Image));
        shade.transform.SetParent(root.transform, false);
        sleepOverlay = shade.GetComponent<Image>();
        sleepOverlay.rectTransform.anchorMin = Vector2.zero;
        sleepOverlay.rectTransform.anchorMax = Vector2.one;
        sleepOverlay.rectTransform.offsetMin = sleepOverlay.rectTransform.offsetMax = Vector2.zero;
        sleepOverlay.color = Color.clear;
    }
    private IEnumerator FadeSleep(float from, float to)
    {
        for (float t = 0; t < 0.6f; t += Time.unscaledDeltaTime)
        {
            sleepOverlay.color = new Color(0, 0, 0, Mathf.Lerp(from, to, t / 0.6f));
            yield return null;
        }
        sleepOverlay.color = new Color(0, 0, 0, to);
    }
    private void PlayKnock()
    {
        GameAudioManager.Instance?.PlayDoorKnock();
    }

    private static GameObject FindObject(string name)
    {
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Include))
            if (t.name == name) return t.gameObject;
        return null;
    }
    private static void SetObject(GameObject obj, bool active)
    {
        if (obj != null && obj.activeSelf != active) obj.SetActive(active);
    }
    private static void ShowPanel(GameObject obj)
    {
        SetObject(obj, true);
        if (obj == null) return;
        CanvasGroup group = obj.GetComponent<CanvasGroup>();
        if (group != null) { group.alpha = 1; group.interactable = true; group.blocksRaycasts = true; }
    }
    private static bool IsVisible(GameObject obj)
    {
        if (obj == null || !obj.activeInHierarchy) return false;
        foreach (CanvasGroup group in obj.GetComponentsInParent<CanvasGroup>())
            if (group.alpha < 0.01f) return false;
        return true;
    }
}

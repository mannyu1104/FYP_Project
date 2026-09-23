using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Controls the main menu, settings panel, and game start flow.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    public static bool IsStartingNewGame { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetNewGameRequest() => IsStartingNewGame = false;

    [System.Serializable]
    private class VisibleLocationPanel
    {
        public string label;
        public GameObject panel;
    }

    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [Tooltip("Button shown only when settings are opened during gameplay.")]
    [SerializeField] private GameObject returnToMainMenuButton;

    [Header("Game Panels")]
    [Tooltip("The main gameplay root that should become visible after starting or loading the game.")]
    [SerializeField] private GameObject gameRootPanel;

    [Header("Save / Load")]
    [Tooltip("Always visible; disabled until a valid save slot exists.")]
    [SerializeField] private GameObject loadGameButton;
    [Tooltip("Optional save system used by the Load Game button.")]
    [SerializeField] private SaveSystem saveSystem;
    [SerializeField] private bool loadInventoryOnLoadGame = true;
    [SerializeField] private bool loadMapItemsOnLoadGame = true;
    [SerializeField] private bool loadUnlockedMapsOnLoadGame = true;
    [SerializeField] private bool loadDialogueProgressOnLoadGame = true;
    [Tooltip("Buttons that create a loadable save record after they are clicked.")]
    [SerializeField] private List<GameObject> saveRecordButtons = new List<GameObject>();
    [SerializeField] private string saveRecordMarkerFileName = "fcj_save_record_marker.json";
    [Header("Transition")]
    [SerializeField] private ScreenTransitionController screenTransitionController;
    [SerializeField] private bool useTransitionOnStartGame = true;

    [Header("Game Settings Button Visibility")]
    [Tooltip("The settings button used during gameplay.")]
    [SerializeField] private GameObject gameSettingsButton;
    [Tooltip("The history button used during gameplay.")]
    [SerializeField] private GameObject gameHistoryButton;
    [SerializeField] private bool placeHistoryButtonBesideSettings = false;
    [SerializeField] private Vector2 historyButtonOffsetFromSettings = new Vector2(130f, 0f);
    [SerializeField] private DialogueController dialogueController;
    [Tooltip("The gameplay location panels where the settings button should be visible.")]
    [SerializeField] private List<VisibleLocationPanel> visibleLocationPanels = new List<VisibleLocationPanel>();

    [Header("Startup")]
    [SerializeField] private bool showMainMenuOnStart = true;
    [SerializeField] private bool pauseLookOnMenu = true;

    private bool settingsOpenedFromGame;
    private bool settingsLookWasPaused;
    private RectTransform historyButtonRect;
    private RectTransform settingsButtonRect;
    private bool historyButtonWasPlaced;
    private Transform historyButtonOriginalParent;
    private int historyButtonOriginalSiblingIndex;
    private Vector2 historyButtonOriginalAnchorMin;
    private Vector2 historyButtonOriginalAnchorMax;
    private Vector2 historyButtonOriginalPivot;
    private Vector2 historyButtonOriginalSizeDelta;
    private Vector2 historyButtonOriginalAnchoredPosition;
    private Vector3 historyButtonOriginalLocalScale;
    private bool historyButtonOriginalStateCached;
    private readonly List<Button> loadButtons = new List<Button>();
    private readonly HashSet<Button> boundSaveRecordButtons = new HashSet<Button>();

    private void Awake()
    {
        ResolveReferences();
        BindSaveRecordButtons();
        foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Include))
            if (button.name == "LoadButton" || button.name == "Load" || button.gameObject == loadGameButton)
            { loadButtons.Add(button); button.onClick = new Button.ButtonClickedEvent(); button.onClick.AddListener(LoadGame); }
        RefreshLoadGameButtonVisibility();
    }

    private void Start()
    {
        if (showMainMenuOnStart)
        {
            ShowMainMenu();
        }
        if (SaveSlotPanel.HasPendingLoad) StartCoroutine(RestoreSlot());
        else if (IsStartingNewGame) StartCoroutine(CompleteNewGame());
    }

    private IEnumerator CompleteNewGame()
    {
        // Allow scene Start methods and the video flow to observe the main menu first.
        yield return null;
        IsStartingNewGame = false;
        StartGameplayImmediately();
        if (dialogueController != null)
        {
            dialogueController.ClearHistory();
            dialogueController.ResetAllNPCProgress();
        }
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.ChangeState(GameState.Normal);
        InvestigationFlowController.Instance?.BeginNewGame();
    }

    private void LateUpdate()
    {
        BindSaveRecordButtons();
        RefreshGameOverlayButtonVisibility();
        RefreshLoadGameButtonVisibility();
    }

    public void StartGame()
    {
        NewGame();
    }

    public void NewGame()
    {
        ResolveReferences();

        if (useTransitionOnStartGame && screenTransitionController != null)
        {
            screenTransitionController.PlayTransition(NewGameImmediately);
            return;
        }

        NewGameImmediately();
    }

    public void LoadGame()
    {
        if (CanLoadGame()) SaveSlotPanel.Open(this, false);
    }

    public void SaveGame() { if (CanOpenGameplaySettings) SaveSlotPanel.Open(this, true); }

    public void CaptureSaveFiles()
    {
        foreach (var board in FindObjectsByType<WhiteBoardSurface>(FindObjectsInactive.Include)) board.SaveLayout();
        ResolveReferences();
        if (saveSystem == null) throw new System.InvalidOperationException("SaveSystem missing");
        saveSystem.SaveInventory();
        saveSystem.SaveInventoryMap();
        saveSystem.SaveMap();
        dialogueController?.SaveDialogueHistory();
        FindAnyObjectByType<ClueManager>(FindObjectsInactive.Include)?.SaveClues();
        FindAnyObjectByType<NotesGrabber>(FindObjectsInactive.Include)?.FlushForSlot();
        InvestigationFlowController.Instance?.SaveProgress();
        WelfareInteractionController.Instance?.SaveProgress();
    }

    public IEnumerator RestoreSlot()
    {
        yield return null;
        yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
        SaveSlotPanel.WritePendingFiles();
        StartGameplayImmediately();
        LoadSavedProgress();
        SaveSlotPanel.RestorePendingState();
    }

    private void NewGameImmediately()
    {
        Scene scene = gameObject.scene;
        if (string.IsNullOrEmpty(scene.path))
        {
            Debug.LogError("Save the scene before starting a new game.", this);
            return;
        }
#if !UNITY_EDITOR
        if (!Application.CanStreamedLevelBeLoaded(scene.path))
        {
            Debug.LogError("The gameplay scene must be included in the build scene list.", this);
            return;
        }
#endif
        IsStartingNewGame = true;
        // A new run must not combine its dialogue with the previous run's inventory.
        DeleteSaveFileIfExists(GetSaveRecordMarkerPath());
        foreach (string path in GetSaveDataPaths()) DeleteSaveFileIfExists(path);
        CountingPoint.ResetTutorialCompletion();
#if UNITY_EDITOR
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
            scene.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
        SceneManager.LoadScene(scene.path);
#endif
    }

    private void StartGameplayImmediately()
    {
        ResolveReferences();
        HideDialogueFloatingUi();
        SetGameObject(mainMenuPanel, false);
        SetGameObject(settingsPanel, false);
        SetGameObject(returnToMainMenuButton, false);
        SetGameObject(gameRootPanel, true);
        settingsOpenedFromGame = false;
        SetLookPaused(false);
        RefreshGameOverlayButtonVisibility();
    }

    private void LoadSavedProgress()
    {
        ResolveReferences();

        if (saveSystem != null)
        {
            if (loadInventoryOnLoadGame)
            {
                saveSystem.LoadGame();
            }

            if (loadMapItemsOnLoadGame)
            {
                saveSystem.LoadGameItemLock();
            }

            if (loadUnlockedMapsOnLoadGame)
            {
                saveSystem.LoadGameMap();
            }
        }
        else
        {
            Debug.LogWarning("MainMenuController: SaveSystem is not assigned, so Load Game only opened gameplay.", this);
        }

        if (loadDialogueProgressOnLoadGame && dialogueController != null)
        {
            dialogueController.LoadDialogueHistory();
        }
        ClueManager clues = FindAnyObjectByType<ClueManager>(FindObjectsInactive.Include);
        if (clues != null) clues.LoadClues();
        if (!SaveSlotPanel.HasPendingLoad) InvestigationFlowController.Instance?.LoadProgress();
    }

    public void OpenSettings()
    {
        ResolveReferences();
        HideDialogueFloatingUi();

        EnsurePanelBlocksRaycasts(settingsPanel);
        settingsOpenedFromGame = false;
        SetGameObject(settingsPanel, true);
        EnsurePanelBlocksRaycasts(settingsPanel);
        settingsPanel.transform.SetAsLastSibling();
        SetGameObject(returnToMainMenuButton, false);
        RefreshGameOverlayButtonVisibility();
    }

    public void OpenSettingsFromGame()
    {
        if (!CanOpenGameplaySettings || IsSettingsVisible) return;
        ResolveReferences();
        HideDialogueFloatingUi();

        EnsurePanelBlocksRaycasts(settingsPanel);
        settingsOpenedFromGame = true;
        LookController look = FindAnyObjectByType<LookController>();
        settingsLookWasPaused = look != null && look.IsPaused;
        SetGameObject(settingsPanel, true);
        EnsurePanelBlocksRaycasts(settingsPanel);
        settingsPanel.transform.SetAsLastSibling();
        SetGameObject(returnToMainMenuButton, true);
        SetLookPaused(true);
        RefreshGameOverlayButtonVisibility();
    }

    public void CloseSettings()
    {
        ResolveReferences();

        SetGameObject(settingsPanel, false);
        SetGameObject(returnToMainMenuButton, false);

        if (settingsOpenedFromGame)
        {
            settingsOpenedFromGame = false;
            SetLookPaused(settingsLookWasPaused);
        }

        RefreshGameOverlayButtonVisibility();
    }

    public void ReturnToMainMenu()
    {
        foreach (var map in FindObjectsByType<MapButton>(FindObjectsInactive.Include)) map.CloseMap();
        foreach (var board in FindObjectsByType<WhiteBoard>(FindObjectsInactive.Include)) board.CloseWhiteBoard();
        InvestigationFlowController.Instance?.StopForMenu();
        settingsOpenedFromGame = false;
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        ResolveReferences();
        HideDialogueFloatingUi();

        SetGameObject(mainMenuPanel, true);
        SetGameObject(settingsPanel, false);
        SetGameObject(returnToMainMenuButton, false);
        SetGameObject(gameRootPanel, false);

        SetLookPaused(pauseLookOnMenu);
        RefreshGameOverlayButtonVisibility();
        RefreshLoadGameButtonVisibility();
    }

    public bool IsMenuVisible => mainMenuPanel != null && mainMenuPanel.activeInHierarchy;
    public bool IsSettingsVisible => settingsPanel != null && settingsPanel.activeInHierarchy;
    public bool IsGameplayVisible => gameRootPanel != null && gameRootPanel.activeInHierarchy && !IsMenuVisible;

    private void RefreshGameOverlayButtonVisibility()
    {
        bool dialogueActive = IsDialogueActive();
        bool historyOpen = IsHistoryOpen();
        EnsureOverlayLayer(gameSettingsButton, 4000);
        SetGameObject(gameSettingsButton, CanOpenGameplaySettings && !IsSettingsVisible && !SaveSlotPanel.IsOpen);
        SetHistoryButtonVisible(dialogueActive || historyOpen);
    }

    private bool CanOpenGameplaySettings
    {
        get
        {
            var computer = FindAnyObjectByType<ComputerCanvasController>(FindObjectsInactive.Include);
            return IsGameplayVisible && !WelfareInteractionController.BlocksSettings &&
                (computer == null || !computer.IsComputerOpen());
        }
    }

    private static void EnsureOverlayLayer(GameObject target, int order)
    {
        if (target == null) return;
        var canvas = target.GetComponent<Canvas>();
        if (canvas == null) canvas = target.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = order;
        if (target.GetComponent<GraphicRaycaster>() == null) target.AddComponent<GraphicRaycaster>();
    }

    private bool IsAnyVisibleLocationPanelActive()
    {
        for (int i = 0; i < visibleLocationPanels.Count; i++)
        {
            GameObject panel = visibleLocationPanels[i].panel;
            if (panel != null && panel.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }

    private void ResolveReferences()
    {
        if (screenTransitionController == null)
        {
            screenTransitionController = FindAnyObjectByType<ScreenTransitionController>();
        }

        if (dialogueController == null)
        {
            dialogueController = FindAnyObjectByType<DialogueController>();
        }

        if (saveSystem == null)
        {
            foreach (SaveSystem candidate in FindObjectsByType<SaveSystem>(FindObjectsInactive.Include))
            {
                if (candidate.inventorymanager != null && candidate.inventoryUsing != null)
                {
                    saveSystem = candidate;
                    break;
                }
            }
        }

        if (screenTransitionController == null)
        {
            GameObject overlayObject = GameObject.Find("BlackTransitionPanel");
            if (overlayObject == null)
            {
                overlayObject = FindSceneObjectByName("BlackTransitionPanel");
            }

            if (overlayObject != null)
            {
                RectTransform overlay = overlayObject.transform as RectTransform;
                CanvasGroup canvasGroup = overlayObject.GetComponent<CanvasGroup>();

                if (canvasGroup == null)
                {
                    canvasGroup = overlayObject.AddComponent<CanvasGroup>();
                }

                screenTransitionController = gameObject.AddComponent<ScreenTransitionController>();
                screenTransitionController.Configure(overlay, canvasGroup);
            }
        }

        if (returnToMainMenuButton == null && settingsPanel != null)
        {
            Transform button = FindChildByName(settingsPanel.transform, "ReturnToMainMenuButton");
            if (button != null)
            {
                returnToMainMenuButton = button.gameObject;
            }
        }

        if (loadGameButton == null || loadGameButton.name != "LoadGameButton")
        {
            GameObject button = GameObject.Find("LoadGameButton");
            if (button == null)
            {
                button = FindSceneObjectByName("LoadGameButton");
            }

            loadGameButton = button;
        }

        if (gameSettingsButton == null)
        {
            GameObject button = GameObject.Find("SettingButton(From InGame)");
            if (button == null)
            {
                button = FindSceneObjectByName("SettingButton(From InGame)");
            }

            gameSettingsButton = button;
        }

        if (gameHistoryButton == null)
        {
            GameObject button = GameObject.Find("HistoryButton");
            if (button == null)
            {
                button = FindSceneObjectByName("HistoryButton");
            }

            gameHistoryButton = button;
        }

        PlaceHistoryButtonBesideSettings();
        FindSaveRecordButtonsIfNeeded();
    }

    private void RefreshLoadGameButtonVisibility()
    {
        bool available = CanLoadGame();
        if (loadGameButton != null) loadGameButton.SetActive(true);
        foreach (Button button in loadButtons)
        {
            if (button == null) continue;
            button.interactable = available;
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.disabledColor = new Color(0.16f, 0.16f, 0.16f, 1f);
            button.colors = colors;
        }
    }

    public void RegisterSaveRecord()
    {
        SaveGame();
    }

    public void DeleteSaveRecordForTest()
    {
        DeleteSaveFileIfExists(GetSaveRecordMarkerPath());

        string[] saveDataPaths = GetSaveDataPaths();
        for (int i = 0; i < saveDataPaths.Length; i++)
        {
            DeleteSaveFileIfExists(saveDataPaths[i]);
        }

        RefreshLoadGameButtonVisibility();
        Debug.Log("MainMenuController: Test save files deleted. Load Game visibility refreshed.", this);
    }

    private void DeleteSaveFileIfExists(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"MainMenuController: Failed to delete save file '{path}'. {exception.Message}", this);
        }
    }

    private bool CanLoadGame()
    {
        return SaveSlotPanel.HasSaves;
    }

    private string GetSaveRecordMarkerPath()
    {
        return Path.Combine(Application.persistentDataPath, saveRecordMarkerFileName);
    }

    private string[] GetSaveDataPaths()
    {
        return new[]
        {
            Path.Combine(Application.persistentDataPath, "inventory.json"),
            Path.Combine(Application.persistentDataPath, "inventorylock.json"),
            Path.Combine(Application.persistentDataPath, "map.json"),
            Path.Combine(Application.persistentDataPath, "dialogue_history.json"),
            Path.Combine(Application.persistentDataPath, "clues.json"),
            Path.Combine(Application.persistentDataPath, "story_progress.json"),
            Path.Combine(Application.persistentDataPath, "notes.json"),
            Path.Combine(Application.persistentDataPath, "whiteboard.json"),
            Path.Combine(Application.persistentDataPath, "welfare_progress.json")
        };
    }

    private void BindSaveRecordButtons()
    {
        FindSaveRecordButtonsIfNeeded();

        for (int i = 0; i < saveRecordButtons.Count; i++)
        {
            GameObject buttonObject = saveRecordButtons[i];
            if (buttonObject == null)
            {
                continue;
            }

            Button button = buttonObject.GetComponent<Button>();
            if (button != null && boundSaveRecordButtons.Add(button))
            {
                button.onClick = new Button.ButtonClickedEvent();
                button.onClick.AddListener(SaveGame);
            }
        }
    }

    private void FindSaveRecordButtonsIfNeeded()
    {
        if (saveRecordButtons.Count > 0)
        {
            return;
        }

        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform target = transforms[i];
            if ((target.name == "SaveButton" || target.name == "Save") && target.gameObject.scene.IsValid())
            {
                saveRecordButtons.Add(target.gameObject);
            }
        }
    }

    private bool IsDialogueActive()
    {
        if (dialogueController == null)
        {
            dialogueController = FindAnyObjectByType<DialogueController>();
        }

        return dialogueController != null && dialogueController.IsDialogueActive;
    }

    private bool IsHistoryOpen()
    {
        if (dialogueController == null)
        {
            dialogueController = FindAnyObjectByType<DialogueController>();
        }

        return dialogueController != null && dialogueController.IsHistoryOpen;
    }

    private void SetHistoryButtonVisible(bool visible)
    {
        if (dialogueController == null)
        {
            dialogueController = FindAnyObjectByType<DialogueController>();
        }

        SetGameObject(gameHistoryButton, visible);

        if (gameHistoryButton != null && visible)
        {
            gameHistoryButton.transform.SetAsLastSibling();
        }

        if (dialogueController != null)
        {
            dialogueController.SetHistoryShortcutVisible(visible);
        }
    }

    private void PlaceHistoryButtonBesideSettings()
    {
        if (!placeHistoryButtonBesideSettings)
        {
            RestoreHistoryButtonPlacement();
            return;
        }

        if (historyButtonWasPlaced || gameHistoryButton == null || gameSettingsButton == null)
        {
            return;
        }

        CacheHistoryButtonPlacement();

        historyButtonRect = gameHistoryButton.transform as RectTransform;
        settingsButtonRect = gameSettingsButton.transform as RectTransform;

        if (historyButtonRect == null || settingsButtonRect == null)
        {
            return;
        }

        historyButtonRect.SetParent(settingsButtonRect.parent, false);
        historyButtonRect.anchorMin = settingsButtonRect.anchorMin;
        historyButtonRect.anchorMax = settingsButtonRect.anchorMax;
        historyButtonRect.pivot = settingsButtonRect.pivot;
        historyButtonRect.sizeDelta = settingsButtonRect.sizeDelta;
        historyButtonRect.anchoredPosition = settingsButtonRect.anchoredPosition + historyButtonOffsetFromSettings;
        historyButtonRect.localScale = settingsButtonRect.localScale;
        historyButtonRect.SetAsLastSibling();

        historyButtonWasPlaced = true;
    }

    private void CacheHistoryButtonPlacement()
    {
        if (historyButtonOriginalStateCached || gameHistoryButton == null)
        {
            return;
        }

        historyButtonRect = gameHistoryButton.transform as RectTransform;
        if (historyButtonRect == null)
        {
            return;
        }

        historyButtonOriginalParent = historyButtonRect.parent;
        historyButtonOriginalSiblingIndex = historyButtonRect.GetSiblingIndex();
        historyButtonOriginalAnchorMin = historyButtonRect.anchorMin;
        historyButtonOriginalAnchorMax = historyButtonRect.anchorMax;
        historyButtonOriginalPivot = historyButtonRect.pivot;
        historyButtonOriginalSizeDelta = historyButtonRect.sizeDelta;
        historyButtonOriginalAnchoredPosition = historyButtonRect.anchoredPosition;
        historyButtonOriginalLocalScale = historyButtonRect.localScale;
        historyButtonOriginalStateCached = true;
    }

    private void RestoreHistoryButtonPlacement()
    {
        if (!historyButtonWasPlaced || !historyButtonOriginalStateCached || gameHistoryButton == null)
        {
            return;
        }

        historyButtonRect = gameHistoryButton.transform as RectTransform;
        if (historyButtonRect == null || historyButtonOriginalParent == null)
        {
            return;
        }

        historyButtonRect.SetParent(historyButtonOriginalParent, false);
        historyButtonRect.anchorMin = historyButtonOriginalAnchorMin;
        historyButtonRect.anchorMax = historyButtonOriginalAnchorMax;
        historyButtonRect.pivot = historyButtonOriginalPivot;
        historyButtonRect.sizeDelta = historyButtonOriginalSizeDelta;
        historyButtonRect.anchoredPosition = historyButtonOriginalAnchoredPosition;
        historyButtonRect.localScale = historyButtonOriginalLocalScale;
        historyButtonRect.SetSiblingIndex(Mathf.Clamp(historyButtonOriginalSiblingIndex, 0, historyButtonOriginalParent.childCount - 1));
        historyButtonWasPlaced = false;
    }

    private void HideDialogueFloatingUi()
    {
        if (dialogueController == null)
        {
            dialogueController = FindAnyObjectByType<DialogueController>();
        }

        if (dialogueController != null)
        {
            dialogueController.HideNonDialogueUi();
        }
    }

    private Transform FindChildByName(Transform parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        if (parent.name == childName)
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform result = FindChildByName(parent.GetChild(i), childName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static GameObject FindSceneObjectByName(string objectName)
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();

        for (int i = 0; i < transforms.Length; i++)
        {
            Transform target = transforms[i];
            if (target.name == objectName && target.gameObject.scene.IsValid())
            {
                return target.gameObject;
            }
        }

        return null;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetLookPaused(bool paused)
    {
        LookController lookController = FindAnyObjectByType<LookController>();
        if (lookController != null)
        {
            lookController.SetPaused(paused);
        }
    }

    private static void SetGameObject(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }

    private static void EnsurePanelBlocksRaycasts(GameObject panel)
    {
        if (panel == null)
        {
            return;
        }

        EnsureOverlayLayer(panel, 4001);
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }
}

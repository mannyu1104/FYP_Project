using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the main menu, settings panel, and game start flow.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    private const string DeleteSaveTestButtonName = "DeleteSaveTestButton";

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
    [Tooltip("Hidden until a save record exists.")]
    [SerializeField] private GameObject loadGameButton;
    [Tooltip("Optional save system used by the Load Game button.")]
    [SerializeField] private SaveSystem saveSystem;
    [SerializeField] private bool loadInventoryOnLoadGame = true;
    [SerializeField] private bool loadMapItemsOnLoadGame = true;
    [SerializeField] private bool loadUnlockedMapsOnLoadGame = true;
    [SerializeField] private bool resetDialogueProgressOnNewGame = true;
    [SerializeField] private bool loadDialogueProgressOnLoadGame = true;
    [Tooltip("Buttons that create a loadable save record after they are clicked.")]
    [SerializeField] private List<GameObject> saveRecordButtons = new List<GameObject>();
    [SerializeField] private string saveRecordMarkerFileName = "fcj_save_record_marker.json";
    [SerializeField] private bool requireSaveDataFileForLoadButton = true;
    [Header("Testing")]
    [Tooltip("Creates a temporary main menu button for deleting save data while testing Load Game visibility.")]
    [SerializeField] private bool showDeleteSaveTestButton = true;
    [SerializeField] private GameObject deleteSaveTestButton;

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
    private readonly HashSet<Button> boundSaveRecordButtons = new HashSet<Button>();

    private void Awake()
    {
        ResolveReferences();
        EnsureDeleteSaveTestButton();
        BindSaveRecordButtons();
        RefreshLoadGameButtonVisibility();
    }

    private void Start()
    {
        if (showMainMenuOnStart)
        {
            ShowMainMenu();
        }
    }

    private void LateUpdate()
    {
        EnsureDeleteSaveTestButton();
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
        ResolveReferences();

        if (!CanLoadGame())
        {
            RefreshLoadGameButtonVisibility();
            return;
        }

        if (useTransitionOnStartGame && screenTransitionController != null)
        {
            screenTransitionController.PlayTransition(LoadGameImmediately);
            return;
        }

        LoadGameImmediately();
    }

    private void NewGameImmediately()
    {
        StartGameplayImmediately();

        if (resetDialogueProgressOnNewGame && dialogueController != null)
        {
            dialogueController.ClearHistory();
            dialogueController.ResetAllNPCProgress();
        }
    }

    private void LoadGameImmediately()
    {
        if (!CanLoadGame())
        {
            RefreshLoadGameButtonVisibility();
            return;
        }

        StartGameplayImmediately();
        LoadSavedProgress();
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
    }

    public void OpenSettings()
    {
        ResolveReferences();
        HideDialogueFloatingUi();

        EnsurePanelBlocksRaycasts(settingsPanel);
        settingsOpenedFromGame = false;
        SetGameObject(settingsPanel, true);
        SetGameObject(returnToMainMenuButton, false);
        RefreshGameOverlayButtonVisibility();
    }

    public void OpenSettingsFromGame()
    {
        ResolveReferences();
        HideDialogueFloatingUi();

        EnsurePanelBlocksRaycasts(settingsPanel);
        settingsOpenedFromGame = true;
        SetGameObject(settingsPanel, true);
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
            SetLookPaused(false);
        }

        RefreshGameOverlayButtonVisibility();
    }

    public void ReturnToMainMenu()
    {
        settingsOpenedFromGame = false;
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        ResolveReferences();
        EnsureDeleteSaveTestButton();
        HideDialogueFloatingUi();

        SetGameObject(mainMenuPanel, true);
        SetGameObject(settingsPanel, false);
        SetGameObject(returnToMainMenuButton, false);
        SetGameObject(gameRootPanel, false);

        SetLookPaused(pauseLookOnMenu);
        RefreshGameOverlayButtonVisibility();
        RefreshLoadGameButtonVisibility();
    }

    private void RefreshGameOverlayButtonVisibility()
    {
        bool dialogueActive = IsDialogueActive();
        bool historyOpen = IsHistoryOpen();
        bool shouldShowGameOverlayButtons = gameRootPanel != null &&
            gameRootPanel.activeInHierarchy &&
            (settingsPanel == null || !settingsPanel.activeInHierarchy) &&
            !dialogueActive &&
            !historyOpen &&
            !MapButton.IsAnyMapOpen &&
            IsAnyVisibleLocationPanelActive();

        SetGameObject(gameSettingsButton, shouldShowGameOverlayButtons);
        SetHistoryButtonVisible(dialogueActive || historyOpen);
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
            saveSystem = FindAnyObjectByType<SaveSystem>();
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

        if (loadGameButton == null)
        {
            GameObject button = GameObject.Find("LoadGameButton");
            if (button == null)
            {
                button = FindSceneObjectByName("LoadGameButton");
            }

            loadGameButton = button;
        }

        if (deleteSaveTestButton == null)
        {
            deleteSaveTestButton = FindSceneObjectByName(DeleteSaveTestButtonName);
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
        SetGameObject(loadGameButton, CanLoadGame());
    }

    public void RegisterSaveRecord()
    {
        try
        {
            File.WriteAllText(GetSaveRecordMarkerPath(), System.DateTime.Now.ToString("O"));
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"MainMenuController: Failed to create save record marker. {exception.Message}", this);
        }

        RefreshLoadGameButtonVisibility();
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

    private void EnsureDeleteSaveTestButton()
    {
        if (!showDeleteSaveTestButton)
        {
            SetGameObject(deleteSaveTestButton, false);
            return;
        }

        if (mainMenuPanel == null)
        {
            return;
        }

        if (deleteSaveTestButton == null)
        {
            Transform existing = FindChildByName(mainMenuPanel.transform, DeleteSaveTestButtonName);
            if (existing != null)
            {
                deleteSaveTestButton = existing.gameObject;
            }
        }

        if (deleteSaveTestButton == null)
        {
            deleteSaveTestButton = CreateDeleteSaveTestButton();
        }

        if (deleteSaveTestButton == null)
        {
            return;
        }

        SetGameObject(deleteSaveTestButton, true);

        Button button = deleteSaveTestButton.GetComponent<Button>();
        if (button != null)
        {
            SetDeleteSaveButtonClick(button);
        }

        TMP_Text text = deleteSaveTestButton.GetComponentInChildren<TMP_Text>(true);
        if (text != null && string.IsNullOrWhiteSpace(text.text))
        {
            text.text = "Delete Save";
        }
    }

    private GameObject CreateDeleteSaveTestButton()
    {
        GameObject template = FindSceneObjectByName("QuitGameButton");
        GameObject buttonObject;

        if (template != null)
        {
            buttonObject = Instantiate(template, mainMenuPanel.transform);
            buttonObject.name = DeleteSaveTestButtonName;
            PositionDeleteSaveButton(buttonObject.transform as RectTransform, template.transform as RectTransform);
        }
        else
        {
            buttonObject = new GameObject(DeleteSaveTestButtonName, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(mainMenuPanel.transform, false);

            RectTransform rectTransform = buttonObject.transform as RectTransform;
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.sizeDelta = new Vector2(260f, 58f);
                rectTransform.anchoredPosition = new Vector2(0f, -180f);
            }

            Image image = buttonObject.GetComponent<Image>();
            if (image != null)
            {
                image.color = Color.white;
            }

            GameObject textObject = new GameObject("Text (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(buttonObject.transform, false);

            RectTransform textRect = textObject.transform as RectTransform;
            if (textRect != null)
            {
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
            }
        }

        TMP_Text text = buttonObject.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            text.text = "Delete Save";
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 28f;
            text.enableAutoSizing = false;
        }

        Button button = buttonObject.GetComponent<Button>();
        if (button != null)
        {
            SetDeleteSaveButtonClick(button);
        }

        buttonObject.SetActive(true);
        return buttonObject;
    }

    private void SetDeleteSaveButtonClick(Button button)
    {
        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(DeleteSaveRecordForTest);
    }

    private void PositionDeleteSaveButton(RectTransform buttonRect, RectTransform templateRect)
    {
        if (buttonRect == null || templateRect == null)
        {
            return;
        }

        buttonRect.anchorMin = templateRect.anchorMin;
        buttonRect.anchorMax = templateRect.anchorMax;
        buttonRect.pivot = templateRect.pivot;
        buttonRect.sizeDelta = templateRect.sizeDelta;
        buttonRect.anchoredPosition = templateRect.anchoredPosition + new Vector2(0f, -70f);
        buttonRect.localScale = templateRect.localScale;
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
        bool markerExists = File.Exists(GetSaveRecordMarkerPath());
        bool saveDataExists = HasAnySaveDataFile();

        if (markerExists && requireSaveDataFileForLoadButton && !saveDataExists)
        {
            DeleteSaveFileIfExists(GetSaveRecordMarkerPath());
            markerExists = false;
        }

        if (requireSaveDataFileForLoadButton)
        {
            return markerExists && saveDataExists;
        }

        return markerExists;
    }

    private string GetSaveRecordMarkerPath()
    {
        return Path.Combine(Application.persistentDataPath, saveRecordMarkerFileName);
    }

    private bool HasAnySaveDataFile()
    {
        string[] saveDataPaths = GetSaveDataPaths();
        for (int i = 0; i < saveDataPaths.Length; i++)
        {
            if (File.Exists(saveDataPaths[i]))
            {
                return true;
            }
        }

        return false;
    }

    private string[] GetSaveDataPaths()
    {
        return new[]
        {
            Path.Combine(Application.persistentDataPath, "inventory.json"),
            Path.Combine(Application.persistentDataPath, "inventorylock.json"),
            Path.Combine(Application.persistentDataPath, "map.json"),
            Path.Combine(Application.persistentDataPath, "dialogue_history.json")
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
                button.onClick.AddListener(RegisterSaveRecord);
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
            if (target.name == "SaveButton" && target.gameObject.scene.IsValid())
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

        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }
}

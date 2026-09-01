using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the main menu, settings panel, and game start flow.
/// </summary>
public class MainMenuController : MonoBehaviour
{
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
    [Tooltip("The main gameplay root that should become visible after pressing Start Game.")]
    [SerializeField] private GameObject gameRootPanel;

    [Header("Transition")]
    [SerializeField] private ScreenTransitionController screenTransitionController;
    [SerializeField] private bool useTransitionOnStartGame = true;

    [Header("Game Settings Button Visibility")]
    [Tooltip("The settings button used during gameplay.")]
    [SerializeField] private GameObject gameSettingsButton;
    [SerializeField] private DialogueController dialogueController;
    [Tooltip("The gameplay location panels where the settings button should be visible.")]
    [SerializeField] private List<VisibleLocationPanel> visibleLocationPanels = new List<VisibleLocationPanel>();

    [Header("Startup")]
    [SerializeField] private bool showMainMenuOnStart = true;
    [SerializeField] private bool pauseLookOnMenu = true;

    private bool settingsOpenedFromGame;

    private void Awake()
    {
        ResolveReferences();
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
        RefreshGameSettingsButtonVisibility();
    }

    public void StartGame()
    {
        ResolveReferences();

        if (useTransitionOnStartGame && screenTransitionController != null)
        {
            screenTransitionController.PlayTransition(StartGameImmediately);
            return;
        }

        StartGameImmediately();
    }

    private void StartGameImmediately()
    {
        ResolveReferences();

        SetGameObject(mainMenuPanel, false);
        SetGameObject(settingsPanel, false);
        SetGameObject(returnToMainMenuButton, false);
        SetGameObject(gameRootPanel, true);
        settingsOpenedFromGame = false;
        SetLookPaused(false);
        RefreshGameSettingsButtonVisibility();
    }

    public void OpenSettings()
    {
        ResolveReferences();

        settingsOpenedFromGame = false;
        SetGameObject(settingsPanel, true);
        SetGameObject(returnToMainMenuButton, false);
        RefreshGameSettingsButtonVisibility();
    }

    public void OpenSettingsFromGame()
    {
        ResolveReferences();

        settingsOpenedFromGame = true;
        SetGameObject(settingsPanel, true);
        SetGameObject(returnToMainMenuButton, true);
        SetLookPaused(true);
        RefreshGameSettingsButtonVisibility();
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

        RefreshGameSettingsButtonVisibility();
    }

    public void ReturnToMainMenu()
    {
        settingsOpenedFromGame = false;
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        ResolveReferences();

        SetGameObject(mainMenuPanel, true);
        SetGameObject(settingsPanel, false);
        SetGameObject(returnToMainMenuButton, false);
        SetGameObject(gameRootPanel, false);

        SetLookPaused(pauseLookOnMenu);
        RefreshGameSettingsButtonVisibility();
    }

    private void RefreshGameSettingsButtonVisibility()
    {
        if (gameSettingsButton == null)
        {
            return;
        }

        bool shouldShow = gameRootPanel != null &&
            gameRootPanel.activeInHierarchy &&
            (settingsPanel == null || !settingsPanel.activeInHierarchy) &&
            !IsDialogueActive() &&
            !MapButton.IsAnyMapOpen &&
            IsAnyVisibleLocationPanelActive();

        SetGameObject(gameSettingsButton, shouldShow);
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
    }

    private bool IsDialogueActive()
    {
        if (dialogueController == null)
        {
            dialogueController = FindAnyObjectByType<DialogueController>();
        }

        return dialogueController != null && dialogueController.IsDialogueActive;
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
}

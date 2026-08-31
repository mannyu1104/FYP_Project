using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the main menu, settings panel, and game start flow.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Game Panels")]
    [Tooltip("The main gameplay root that should become visible after pressing Start Game.")]
    [SerializeField] private GameObject gameRootPanel;
    [Tooltip("Optional panels that should be hidden while the main menu is open.")]
    [SerializeField] private List<GameObject> panelsToHideOnMenu = new List<GameObject>();

    [Header("Startup")]
    [SerializeField] private bool showMainMenuOnStart = true;
    [SerializeField] private bool pauseLookOnMenu = true;

    private void Start()
    {
        if (showMainMenuOnStart)
        {
            ShowMainMenu();
        }
    }

    public void StartGame()
    {
        SetGameObject(mainMenuPanel, false);
        SetGameObject(settingsPanel, false);
        SetGameObject(gameRootPanel, true);
        SetLookPaused(false);
    }

    public void OpenSettings()
    {
        SetGameObject(settingsPanel, true);
    }

    public void CloseSettings()
    {
        SetGameObject(settingsPanel, false);
    }

    public void ShowMainMenu()
    {
        SetGameObject(mainMenuPanel, true);
        SetGameObject(settingsPanel, false);
        SetGameObject(gameRootPanel, false);

        for (int i = 0; i < panelsToHideOnMenu.Count; i++)
        {
            SetGameObject(panelsToHideOnMenu[i], false);
        }

        SetLookPaused(pauseLookOnMenu);
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
        LookController lookController = FindFirstObjectByType<LookController>();
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

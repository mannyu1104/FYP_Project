using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Switches between the main gameplay canvas and the computer canvas.
/// </summary>
public class ComputerCanvasController : MonoBehaviour
{
    [Header("Canvas References")]
    [SerializeField] private GameObject mainGameCanvas;
    [SerializeField] private GameObject computerCanvas;

    [Header("Return Button")]
    [SerializeField] private Button returnToGameButton;

    [Header("Optional")]
    [SerializeField] private LookController lookController;
    [SerializeField] private bool pauseLookWhileComputerOpen = true;

    private bool wasMainGameCanvasActive;
    private bool wasLookPaused;
    private bool isComputerOpen;

    private void Awake()
    {
        ResolveReferences();
        BindReturnButton();
        CloseComputerImmediately();
    }

    private void OnEnable()
    {
        BindReturnButton();
    }

    private void OnDisable()
    {
        if (returnToGameButton != null)
        {
            returnToGameButton.onClick.RemoveListener(CloseComputer);
        }
    }

    public void OpenComputer()
    {
        ResolveReferences();

        if (computerCanvas == null || mainGameCanvas == null)
        {
            Debug.LogWarning("ComputerCanvasController: MainGameCanvas or ComputerCanvas is not assigned.", this);
            return;
        }

        wasMainGameCanvasActive = mainGameCanvas.activeSelf;
        isComputerOpen = true;

        if (lookController != null && pauseLookWhileComputerOpen)
        {
            wasLookPaused = lookController.IsPaused;
            lookController.SetPaused(true);
        }

        mainGameCanvas.SetActive(false);
        computerCanvas.SetActive(true);
    }

    public void CloseComputer()
    {
        ResolveReferences();

        if (computerCanvas != null)
        {
            computerCanvas.SetActive(false);
        }

        if (mainGameCanvas != null)
        {
            mainGameCanvas.SetActive(wasMainGameCanvasActive);
        }

        if (lookController != null && pauseLookWhileComputerOpen)
        {
            lookController.SetPaused(wasLookPaused);
        }

        isComputerOpen = false;
    }

    public bool IsComputerOpen()
    {
        return isComputerOpen;
    }

    private void CloseComputerImmediately()
    {
        if (computerCanvas != null)
        {
            computerCanvas.SetActive(false);
        }
    }

    private void ResolveReferences()
    {
        if (lookController == null)
        {
            lookController = FindAnyObjectByType<LookController>();
        }
    }

    private void BindReturnButton()
    {
        if (returnToGameButton == null)
        {
            return;
        }

        returnToGameButton.onClick.RemoveListener(CloseComputer);
        returnToGameButton.onClick.AddListener(CloseComputer);
    }
}

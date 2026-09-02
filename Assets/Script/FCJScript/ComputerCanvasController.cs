using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Switches between the main gameplay canvas and the computer canvas.
/// </summary>
public class ComputerCanvasController : MonoBehaviour
{
    private static readonly Vector2 VideoReturnButtonSize = new Vector2(58f, 58f);
    private static readonly Vector2 VideoReturnButtonTopRightOffset = new Vector2(-24f, -24f);

    [Header("Canvas References")]
    [SerializeField] private GameObject mainGameCanvas;
    [SerializeField] private GameObject computerCanvas;

    [Header("Return Button")]
    [SerializeField] private Button returnToGameButton;
    [SerializeField] private string returnButtonName = "BackToMainGameButton";
    [SerializeField] private bool repairReturnButtonOnOpen = true;
    [SerializeField] private string[] panelsThatHideReturnButton =
    {
        "Internet Discovery Panel",
        "Tutorial Panel"
    };

    [Header("Optional")]
    [SerializeField] private LookController lookController;
    [SerializeField] private bool pauseLookWhileComputerOpen = true;

    private bool wasMainGameCanvasActive;
    private bool wasLookPaused;
    private bool isComputerOpen;
    private bool hideReturnButtonUntilBrowserCloses;

    private void Awake()
    {
        ResolveReferences();
        EnsureReturnButtonReady();
        BindReturnButton();
        CloseComputerImmediately();
    }

    private void OnEnable()
    {
        BrowserAppButton.BrowserAppOpened += OnBrowserAppOpened;
        EnsureReturnButtonReady();
        BindReturnButton();
    }

    private void OnDisable()
    {
        BrowserAppButton.BrowserAppOpened -= OnBrowserAppOpened;

        if (returnToGameButton != null)
        {
            returnToGameButton.onClick.RemoveListener(CloseComputer);
        }
    }

    private void LateUpdate()
    {
        RefreshReturnButtonVisibility();
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
        hideReturnButtonUntilBrowserCloses = false;

        if (lookController != null && pauseLookWhileComputerOpen)
        {
            wasLookPaused = lookController.IsPaused;
            lookController.SetPaused(true);
        }

        mainGameCanvas.SetActive(false);
        computerCanvas.SetActive(true);

        EnsureReturnButtonReady();
        BindReturnButton();
        RefreshReturnButtonVisibility();
    }

    public void CloseComputer()
    {
        ResolveReferences();

        if (computerCanvas != null)
        {
            computerCanvas.SetActive(false);
        }

        SetReturnButtonVisible(false);

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

    private void EnsureReturnButtonReady()
    {
        if (!repairReturnButtonOnOpen || computerCanvas == null)
        {
            return;
        }

        if (returnToGameButton == null)
        {
            Transform found = FindChildByName(computerCanvas.transform, returnButtonName);
            if (found != null)
            {
                returnToGameButton = found.GetComponent<Button>();
            }
        }

        if (returnToGameButton == null)
        {
            returnToGameButton = CreateReturnButton();
        }

        if (!returnToGameButton.transform.IsChildOf(computerCanvas.transform))
        {
            returnToGameButton.transform.SetParent(computerCanvas.transform, false);
        }

        returnToGameButton.gameObject.name = returnButtonName;
        returnToGameButton.gameObject.SetActive(true);
        returnToGameButton.interactable = true;
        returnToGameButton.transform.SetAsLastSibling();

        RectTransform rectTransform = returnToGameButton.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(1f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(1f, 1f);
            rectTransform.anchoredPosition = VideoReturnButtonTopRightOffset;
            rectTransform.sizeDelta = VideoReturnButtonSize;
            rectTransform.localScale = Vector3.one;
        }

        Image buttonImage = returnToGameButton.GetComponent<Image>();
        if (buttonImage == null)
        {
            buttonImage = returnToGameButton.gameObject.AddComponent<Image>();
        }

        buttonImage.color = new Color(0.95f, 0.18f, 0.18f, 1f);
        buttonImage.raycastTarget = true;

        CanvasGroup canvasGroup = returnToGameButton.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        TMP_Text label = returnToGameButton.GetComponentInChildren<TMP_Text>(true);
        if (label == null)
        {
            label = CreateReturnButtonLabel(returnToGameButton.transform);
        }

        label.gameObject.SetActive(true);
        label.text = "X";
        label.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        label.fontSize = 34f;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;

        RectTransform labelRect = label.GetComponent<RectTransform>();
        if (labelRect != null)
        {
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            labelRect.localScale = Vector3.one;
        }
    }

    private Button CreateReturnButton()
    {
        GameObject buttonObject = new GameObject(returnButtonName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(computerCanvas.transform, false);

        Button button = buttonObject.GetComponent<Button>();
        CreateReturnButtonLabel(buttonObject.transform);
        return button;
    }

    private TMP_Text CreateReturnButtonLabel(Transform parent)
    {
        GameObject labelObject = new GameObject("Text (TMP)", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(parent, false);
        return labelObject.GetComponent<TextMeshProUGUI>();
    }

    private Transform FindChildByName(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }

            Transform found = FindChildByName(child, childName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private void RefreshReturnButtonVisibility()
    {
        if (returnToGameButton == null)
        {
            return;
        }

        SetReturnButtonVisible(IsComputerDesktopVisible());
    }

    private bool IsComputerDesktopVisible()
    {
        if (!isComputerOpen || computerCanvas == null || !computerCanvas.activeInHierarchy)
        {
            return false;
        }

        if (hideReturnButtonUntilBrowserCloses)
        {
            if (BrowserTabManager.Instance != null && BrowserTabManager.Instance.ActiveTab == null)
            {
                hideReturnButtonUntilBrowserCloses = false;
            }
            else
            {
                return false;
            }
        }

        if (BrowserTabManager.Instance != null && BrowserTabManager.Instance.ActiveTab != null)
        {
            return false;
        }

        for (int i = 0; i < panelsThatHideReturnButton.Length; i++)
        {
            Transform panel = FindChildByName(computerCanvas.transform, panelsThatHideReturnButton[i]);
            if (IsVisiblePanel(panel))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsVisiblePanel(Transform panel)
    {
        if (panel == null || !panel.gameObject.activeInHierarchy)
        {
            return false;
        }

        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            return true;
        }

        return canvasGroup.alpha > 0.01f;
    }

    private void SetReturnButtonVisible(bool visible)
    {
        if (returnToGameButton != null && returnToGameButton.gameObject.activeSelf != visible)
        {
            returnToGameButton.gameObject.SetActive(visible);
        }
    }

    private void OnBrowserAppOpened()
    {
        hideReturnButtonUntilBrowserCloses = true;
        SetReturnButtonVisible(false);
    }
}

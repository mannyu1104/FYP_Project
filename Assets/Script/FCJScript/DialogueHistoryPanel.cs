using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class DialogueHistoryPanel : MonoBehaviour
{
    [Header("History Content")]
    [HideInInspector]
    public Text historyText;
    public TMP_Text tmpHistoryText;
    public ScrollRect scrollRect;
    public Scrollbar verticalScrollbar;

    [Header("Keyboard Scrolling")]
    [Min(0.1f)]
    public float keyboardScrollSpeed = 0.8f;

    [Header("Mouse Scrolling")]
    [Min(0.1f)]
    public float verticalScrollSpeed = 20f;

    [Header("Dimmed Background")]
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.65f);

    void OnValidate()
    {
        keyboardScrollSpeed = Mathf.Max(0.1f, keyboardScrollSpeed);
        verticalScrollSpeed = Mathf.Max(0.1f, verticalScrollSpeed);
        ConfigureScrollRect();
    }

    void Awake()
    {
        Image background = GetComponent<Image>();

        if (background != null)
        {
            background.color = backgroundColor;
            background.raycastTarget = true;
        }

        ConfigureScrollRect();
    }

    public void SetEntries(IReadOnlyList<string> entries)
    {
        StringBuilder content = new StringBuilder();

        for (int i = 0; i < entries.Count; i++)
        {
            content.AppendLine(entries[i]);
            content.AppendLine();
        }

        if (historyText != null)
        {
            historyText.text = content.ToString();
        }

        if (tmpHistoryText != null)
        {
            tmpHistoryText.text = content.ToString();
        }

        RefreshScrollContent();
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy || scrollRect == null)
        {
            return;
        }

        float direction = GetKeyboardScrollDirection();
        if (direction != 0f)
        {
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(
                scrollRect.verticalNormalizedPosition + direction * keyboardScrollSpeed * Time.unscaledDeltaTime
            );
        }

        scrollRect.horizontalNormalizedPosition = 0.5f;
    }

    public void Toggle()
    {
        SetVisible(!gameObject.activeSelf);
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);

        if (visible && scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            RefreshScrollContent();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private void RefreshScrollContent()
    {
        if (scrollRect == null || scrollRect.content == null)
        {
            return;
        }

        RectTransform content = scrollRect.content;
        RectTransform viewport = scrollRect.viewport;

        if (viewport != null)
        {
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, viewport.rect.width);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
        }

        if (tmpHistoryText != null && viewport != null)
        {
            ConfigureTextLayout(tmpHistoryText.rectTransform);
            tmpHistoryText.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                viewport.rect.width
            );
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        float requiredHeight = 0f;
        if (tmpHistoryText != null)
        {
            tmpHistoryText.ForceMeshUpdate();
            requiredHeight = tmpHistoryText.preferredHeight;

            tmpHistoryText.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                requiredHeight
            );
        }
        else if (historyText != null)
        {
            ConfigureTextLayout(historyText.rectTransform);
            requiredHeight = historyText.preferredHeight;

            historyText.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                requiredHeight
            );
        }

        float viewportHeight = viewport != null ? viewport.rect.height : 0f;
        content.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            Mathf.Max(viewportHeight, requiredHeight)
        );

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        Canvas.ForceUpdateCanvases();
        scrollRect.SetLayoutVertical();
        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
    }

    private void ConfigureScrollRect()
    {
        if (scrollRect == null)
        {
            return;
        }

        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = verticalScrollSpeed;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.verticalScrollbar = verticalScrollbar != null
            ? verticalScrollbar
            : FindVerticalScrollbar();
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        scrollRect.horizontalScrollbar = null;
    }

    private Scrollbar FindVerticalScrollbar()
    {
        Scrollbar[] scrollbars = GetComponentsInChildren<Scrollbar>(true);

        for (int i = 0; i < scrollbars.Length; i++)
        {
            if (scrollbars[i].direction == Scrollbar.Direction.BottomToTop ||
                scrollbars[i].direction == Scrollbar.Direction.TopToBottom)
            {
                return scrollbars[i];
            }
        }

        return null;
    }

    private void ConfigureTextLayout(RectTransform textRect)
    {
        textRect.anchorMin = new Vector2(0f, 1f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.pivot = new Vector2(0.5f, 1f);
        textRect.anchoredPosition = Vector2.zero;
    }

    private float GetKeyboardScrollDirection()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.upArrowKey.isPressed)
            {
                return 1f;
            }

            if (Keyboard.current.downArrowKey.isPressed)
            {
                return -1f;
            }

            return 0f;
        }
#endif

        if (Input.GetKey(KeyCode.UpArrow))
        {
            return 1f;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            return -1f;
        }

        return 0f;
    }
}

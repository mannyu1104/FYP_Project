using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueHistoryPanel : MonoBehaviour
{
    [Header("History Content")]
    [HideInInspector]
    public Text historyText;
    public TMP_Text tmpHistoryText;
    public ScrollRect scrollRect;

    [Header("Dimmed Background")]
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.65f);

    void Awake()
    {
        Image background = GetComponent<Image>();

        if (background != null)
        {
            background.color = backgroundColor;
            background.raycastTarget = true;
        }

        SetVisible(false);
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
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class DialogueController : MonoBehaviour
{
    [System.Serializable]
    private class DialogueSaveData
    {
        public List<string> historyEntries = new List<string>();
        public List<NPCSaveData> npcProgress = new List<NPCSaveData>();
    }

    [System.Serializable]
    private class NPCSaveData
    {
        public string saveId;
        public int nextConversationIndex;
        public bool hasTalked;
    }

    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        [TextArea(2, 6)]
        public string dialogueText;
    }

    [Header("Core References")]
    public LookController lookController;
    [Tooltip("Optional parent of the whole dialogue UI. It stays hidden until a conversation starts.")]
    public RectTransform dialogueUIRoot;
    public RectTransform dialoguePanel;

    [Header("Dialogue Text")]
    [Tooltip("Use these fields when your text objects use TextMeshPro.")]
    public TMP_Text tmpSpeakerText;
    public TMP_Text tmpDialogueText;
    public TMP_FontAsset chineseFontAsset;

    [Header("History UI")]
    public RectTransform historyPanel;
    public DialogueHistoryPanel historyController;
    public TMP_Text tmpHistoryText;
    public ScrollRect historyScrollRect;
    public Button historyButton;

    [Header("Dialogue Buttons")]
    public Button autoButton;
    public Button skipButton;

    [Header("Optional Legacy Text")]
    [Tooltip("Only assign these when using the legacy UI Text component instead of TextMeshPro.")]
    [HideInInspector]
    public Text speakerText;
    [HideInInspector]
    public Text dialogueText;
    [HideInInspector]
    public Text historyText;

    [Header("Dialogue Layout")]
    public bool autoFitText = true;
    [Min(8f)]
    public float dialogueFontSize = 32f;
    [Min(100f)]
    public float maximumTextWidth = 1200f;

    [Header("Auto Mode")]
    [Min(0.1f)]
    public float autoAdvanceSeconds = 3f;

    [Header("Typewriter Settings")]
    public bool enableTypewriterEffect = true;
    [Min(1f)]
    public float charactersPerSecond = 30f;
    [Tooltip("Add or remove speed presets here. Values mean characters displayed per second.")]
    public List<float> typingSpeedPresets = new List<float> { 15f, 30f, 60f, 120f };
    [Min(0)]
    public int activeSpeedPreset = 1;

    [Header("Item Inspect")]
    public GameObject itemInspectRoot;
    public Image itemInspectImage;
    public TMP_Text itemInspectTitleText;

    [Header("Save")]
    public bool saveHistoryAutomatically = true;
    public bool loadHistoryAutomatically = true;

    private readonly List<string> historyEntries = new List<string>();
    private List<DialogueLine> currentLines;
    private NPCDialogueTrigger currentNpc;
    private Coroutine autoRoutine;
    private int currentLineIndex = -1;
    private bool isAutoMode;
    private bool isDialogueActive;
    private bool suppressAdvance;
    private bool lookWasPaused;
    private HoverEffect[] hoverEffects;
    private string dialogueSavePath;
    private Coroutine typingRoutine;
    private bool isTyping;
    private string currentDialogueText = string.Empty;

    public bool IsDialogueActive => isDialogueActive;
    public bool IsAutoMode => isAutoMode;

    void Awake()
    {
        dialogueSavePath = Path.Combine(Application.persistentDataPath, "dialogue_history.json");

        if (lookController == null)
        {
            lookController = FindFirstObjectByType<LookController>();
        }

        hoverEffects = FindObjectsByType<HoverEffect>(FindObjectsSortMode.None);

        historyButton = historyButton != null ? historyButton : FindButton("HistoryButton");
        autoButton = autoButton != null ? autoButton : FindButton("AutoButton");
        skipButton = skipButton != null ? skipButton : FindButton("SkipButton");

        SetPanelVisible(dialoguePanel, false);
        SetPanelVisible(historyPanel, false);

        if (dialogueUIRoot == null && dialoguePanel != null)
        {
            dialogueUIRoot = dialoguePanel.parent as RectTransform;
        }

        SetPanelVisible(dialogueUIRoot, false);

        if (historyController == null && historyPanel != null)
        {
            historyController = historyPanel.GetComponent<DialogueHistoryPanel>();
        }

        ApplyChineseFont();

        if (loadHistoryAutomatically)
        {
            LoadDialogueHistory();
        }

        SetButtonVisible(historyButton, false);
        SetButtonVisible(autoButton, false);
        SetButtonVisible(skipButton, false);

        if (historyButton != null)
        {
            historyButton.onClick.AddListener(ToggleHistory);
        }

        if (autoButton != null)
        {
            autoButton.onClick.AddListener(ToggleAutoMode);
        }

        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipDialogue);
        }
    }

    void OnValidate()
    {
        charactersPerSecond = Mathf.Max(1f, charactersPerSecond);
        activeSpeedPreset = Mathf.Max(0, activeSpeedPreset);

        if (typingSpeedPresets == null)
        {
            typingSpeedPresets = new List<float>();
        }

        for (int i = 0; i < typingSpeedPresets.Count; i++)
        {
            typingSpeedPresets[i] = Mathf.Max(1f, typingSpeedPresets[i]);
        }
    }

    void Update()
    {
        if (!isDialogueActive)
        {
            return;
        }

        if (WasHistoryTogglePressed())
        {
            ToggleHistory();
            return;
        }

        if (IsHistoryOpen())
        {
            if (WasHistoryClosePressed())
            {
                ToggleHistory();
            }

            return;
        }

        if (suppressAdvance)
        {
            suppressAdvance = false;
            return;
        }

        if (WasAdvancePressed())
        {
            AdvanceDialogue();
        }
    }

    public void StartConversation(List<DialogueLine> lines, NPCDialogueTrigger npc)
    {
        if (isDialogueActive || lines == null || lines.Count == 0)
        {
            return;
        }

        if (npc != null)
        {
            HideItemInspect();
        }

        currentLines = lines;
        currentNpc = npc;
        currentLineIndex = -1;
        isDialogueActive = true;
        isAutoMode = false;
        suppressAdvance = true;

        if (historyEntries.Count > 0)
        {
            historyEntries.Add(string.Empty);
        }

        UpdateHistoryText();
        SetPanelVisible(dialogueUIRoot, true);
        SetPanelVisible(historyPanel, false);
        SetPanelVisible(dialoguePanel, true);
        SetButtonVisible(historyButton, true);
        SetButtonVisible(autoButton, true);
        SetButtonVisible(skipButton, true);

        if (lookController != null)
        {
            lookWasPaused = lookController.IsPaused;
            lookController.SetPaused(true);
        }

        SetHoverEffectsPaused(true);

        AdvanceDialogue();
    }

    public void AdvanceDialogue()
    {
        if (!isDialogueActive)
        {
            return;
        }

        if (isTyping)
        {
            SetDialogueText(currentDialogueText);
            StopTypingRoutine();
            return;
        }

        currentLineIndex++;

        if (currentLineIndex >= currentLines.Count)
        {
            FinishDialogue();
            return;
        }

        StopTypingRoutine();

        DialogueLine line = currentLines[currentLineIndex];
        currentDialogueText = line.dialogueText;
        if (speakerText != null)
        {
            speakerText.text = line.speakerName;
        }

        if (tmpSpeakerText != null)
        {
            tmpSpeakerText.text = line.speakerName;
        }

        if (dialogueText != null)
        {
            dialogueText.fontSize = Mathf.RoundToInt(dialogueFontSize);

            if (autoFitText)
            {
                dialogueText.rectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Horizontal,
                    maximumTextWidth
                );
            }
        }

        if (tmpDialogueText != null)
        {
            tmpDialogueText.fontSize = dialogueFontSize;

            if (autoFitText)
            {
                tmpDialogueText.rectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Horizontal,
                    maximumTextWidth
                );
            }
        }

        if (enableTypewriterEffect)
        {
            typingRoutine = StartCoroutine(TypeDialogue(line.dialogueText));
        }
        else
        {
            SetDialogueText(line.dialogueText);
        }

        historyEntries.Add(string.IsNullOrEmpty(line.speakerName)
            ? line.dialogueText
            : line.speakerName + ": " + line.dialogueText);
        UpdateHistoryText();
        SaveDialogueHistoryIfEnabled();

        if (isAutoMode)
        {
            RestartAutoRoutine();
        }
    }

    public void ToggleHistory()
    {
        if (!isDialogueActive || historyPanel == null)
        {
            return;
        }

        bool openingHistory = !historyPanel.gameObject.activeSelf;

        if (openingHistory && isAutoMode && autoRoutine != null)
        {
            StopCoroutine(autoRoutine);
            autoRoutine = null;
        }

        if (historyController != null)
        {
            historyController.Toggle();
        }
        else
        {
            SetPanelVisible(historyPanel, !historyPanel.gameObject.activeSelf);
        }
        suppressAdvance = true;

        if (historyPanel.gameObject.activeSelf && historyScrollRect != null && historyController == null)
        {
            Canvas.ForceUpdateCanvases();
            historyScrollRect.verticalNormalizedPosition = 0f;
        }

        if (!historyPanel.gameObject.activeSelf && isAutoMode)
        {
            RestartAutoRoutine();
        }

        if (openingHistory && historyButton != null)
        {
            Transform buttonParent = historyButton.transform.parent;

            if (buttonParent != historyPanel.parent)
            {
                historyButton.transform.SetParent(historyPanel.parent, true);
            }

            historyButton.transform.SetAsLastSibling();
        }
    }

    private bool IsHistoryOpen()
    {
        return historyPanel != null && historyPanel.gameObject.activeSelf;
    }

    public void ToggleAutoMode()
    {
        if (!isDialogueActive)
        {
            return;
        }

        isAutoMode = !isAutoMode;
        suppressAdvance = true;

        if (isAutoMode)
        {
            RestartAutoRoutine();
        }
        else if (autoRoutine != null)
        {
            StopCoroutine(autoRoutine);
            autoRoutine = null;
        }
    }

    public void SkipDialogue()
    {
        if (!isDialogueActive)
        {
            return;
        }

        AddSkippedLinesToHistory();
        suppressAdvance = true;
        FinishDialogue();
    }

    private void AddSkippedLinesToHistory()
    {
        if (currentLines == null)
        {
            return;
        }

        for (int i = currentLineIndex + 1; i < currentLines.Count; i++)
        {
            DialogueLine line = currentLines[i];
            string entry = string.IsNullOrEmpty(line.speakerName)
                ? line.dialogueText
                : line.speakerName + ": " + line.dialogueText;

            historyEntries.Add(entry);
        }

        UpdateHistoryText();
        SaveDialogueHistoryIfEnabled();
    }

    private void FinishDialogue()
    {
        StopTypingRoutine();
        HideItemInspect();

        if (autoRoutine != null)
        {
            StopCoroutine(autoRoutine);
            autoRoutine = null;
        }

        if (currentNpc != null)
        {
            currentNpc.MarkTalked();
            SaveDialogueHistoryIfEnabled();
        }

        isDialogueActive = false;
        isAutoMode = false;
        currentLines = null;
        currentNpc = null;
        SetPanelVisible(dialoguePanel, false);
        SetPanelVisible(historyPanel, false);
        SetPanelVisible(dialogueUIRoot, false);
        SetButtonVisible(historyButton, false);
        SetButtonVisible(autoButton, false);
        SetButtonVisible(skipButton, false);

        if (lookController != null)
        {
            lookController.SetPaused(lookWasPaused);
        }

        SetHoverEffectsPaused(false);
    }

    private void RestartAutoRoutine()
    {
        if (autoRoutine != null)
        {
            StopCoroutine(autoRoutine);
        }

        autoRoutine = StartCoroutine(AutoAdvanceRoutine());
    }

    private IEnumerator AutoAdvanceRoutine()
    {
        while (isTyping)
        {
            yield return null;
        }

        yield return new WaitForSecondsRealtime(autoAdvanceSeconds);

        if (isDialogueActive && isAutoMode)
        {
            AdvanceDialogue();
        }
    }

    private void UpdateHistoryText()
    {
        if (historyText == null && tmpHistoryText == null)
        {
            return;
        }

        StringBuilder content = new StringBuilder();
        for (int i = 0; i < historyEntries.Count; i++)
        {
            content.AppendLine(historyEntries[i]);
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

        if (historyController != null)
        {
            historyController.SetEntries(historyEntries);
        }
    }

    private IEnumerator TypeDialogue(string text)
    {
        isTyping = true;
        SetDialogueText(string.Empty);

        float speed = GetActiveTypingSpeed();
        float characterTimer = 0f;
        int visibleCharacterCount = 0;

        while (visibleCharacterCount < text.Length)
        {
            characterTimer += Time.unscaledDeltaTime * speed;
            int targetCharacterCount = Mathf.Min(text.Length, Mathf.FloorToInt(characterTimer));

            if (targetCharacterCount > visibleCharacterCount)
            {
                visibleCharacterCount = targetCharacterCount;
                SetDialogueText(text.Substring(0, visibleCharacterCount));
            }

            yield return null;
        }

        SetDialogueText(text);
        isTyping = false;
        typingRoutine = null;
    }

    private void SetDialogueText(string text)
    {
        if (dialogueText != null)
        {
            dialogueText.text = text;
        }

        if (tmpDialogueText != null)
        {
            tmpDialogueText.text = text;
        }
    }

    private float GetActiveTypingSpeed()
    {
        if (typingSpeedPresets != null && typingSpeedPresets.Count > 0)
        {
            int presetIndex = Mathf.Clamp(activeSpeedPreset, 0, typingSpeedPresets.Count - 1);
            return Mathf.Max(1f, typingSpeedPresets[presetIndex]);
        }

        return Mathf.Max(1f, charactersPerSecond);
    }

    private void StopTypingRoutine()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        isTyping = false;
    }

    private void SetPanelVisible(RectTransform panel, bool visible)
    {
        if (panel != null)
        {
            panel.gameObject.SetActive(visible);
        }
    }

    private void SetButtonVisible(Button button, bool visible)
    {
        if (button != null)
        {
            button.gameObject.SetActive(visible);
        }
    }

    private Button FindButton(string objectName)
    {
        GameObject buttonObject = GameObject.Find(objectName);
        return buttonObject != null ? buttonObject.GetComponent<Button>() : null;
    }

    public void ShowItemInspect(CursorInteractionTarget target)
    {
        if (target == null)
        {
            return;
        }

        ShowItemInspect(target.itemName, target.itemDescription, target.itemSprite, target.showInspectImage);
    }

    public void ShowItemInspect(string title, string description, Sprite sprite, bool showCenterImage = true)
    {
        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(description) && sprite == null)
        {
            return;
        }

        string safeTitle = string.IsNullOrWhiteSpace(title) ? "Item" : title;
        string safeDescription = string.IsNullOrWhiteSpace(description)
            ? "This item has no description yet."
            : description;

        if (itemInspectTitleText != null)
        {
            itemInspectTitleText.text = safeTitle;
        }

        if (itemInspectImage != null)
        {
            itemInspectImage.sprite = sprite;
            itemInspectImage.gameObject.SetActive(showCenterImage && sprite != null);
        }

        if (itemInspectRoot != null)
        {
            itemInspectRoot.SetActive(showCenterImage && sprite != null || !showCenterImage);
        }

        List<DialogueLine> itemLines = new List<DialogueLine>
        {
            new DialogueLine
            {
                speakerName = safeTitle,
                dialogueText = safeDescription
            }
        };

        StartConversation(itemLines, null);
    }

    public void HideItemInspect()
    {
        if (itemInspectImage != null)
        {
            itemInspectImage.sprite = null;
            itemInspectImage.gameObject.SetActive(false);
        }

        if (itemInspectRoot != null)
        {
            itemInspectRoot.SetActive(false);
        }
    }

    public void ClearHistory()
    {
        historyEntries.Clear();
        UpdateHistoryText();
        SaveDialogueHistoryIfEnabled();
    }

    public void SaveDialogueHistory()
    {
        if (string.IsNullOrEmpty(dialogueSavePath))
        {
            dialogueSavePath = Path.Combine(Application.persistentDataPath, "dialogue_history.json");
        }

        DialogueSaveData data = new DialogueSaveData
        {
            historyEntries = new List<string>(historyEntries),
            npcProgress = CreateNPCSaveData()
        };

        File.WriteAllText(dialogueSavePath, JsonUtility.ToJson(data, true));
    }

    public void LoadDialogueHistory()
    {
        if (string.IsNullOrEmpty(dialogueSavePath))
        {
            dialogueSavePath = Path.Combine(Application.persistentDataPath, "dialogue_history.json");
        }

        if (!File.Exists(dialogueSavePath))
        {
            return;
        }

        DialogueSaveData data = JsonUtility.FromJson<DialogueSaveData>(File.ReadAllText(dialogueSavePath));

        if (data == null)
        {
            return;
        }

        historyEntries.Clear();

        if (data.historyEntries != null)
        {
            historyEntries.AddRange(data.historyEntries);
        }

        RestoreNPCProgress(data.npcProgress);
        UpdateHistoryText();
    }

    public void ResetAllNPCProgress()
    {
        if (isDialogueActive)
        {
            return;
        }

        NPCDialogueTrigger[] npcs = FindObjectsByType<NPCDialogueTrigger>(FindObjectsSortMode.None);

        for (int i = 0; i < npcs.Length; i++)
        {
            npcs[i].ResetProgress();
        }

        SaveDialogueHistory();
    }

    private List<NPCSaveData> CreateNPCSaveData()
    {
        NPCDialogueTrigger[] npcs = FindObjectsByType<NPCDialogueTrigger>(FindObjectsSortMode.None);
        List<NPCSaveData> data = new List<NPCSaveData>();

        for (int i = 0; i < npcs.Length; i++)
        {
            data.Add(new NPCSaveData
            {
                saveId = npcs[i].SaveId,
                nextConversationIndex = npcs[i].NextConversationIndex,
                hasTalked = npcs[i].HasTalked
            });
        }

        return data;
    }

    private void RestoreNPCProgress(List<NPCSaveData> savedProgress)
    {
        if (savedProgress == null)
        {
            return;
        }

        NPCDialogueTrigger[] npcs = FindObjectsByType<NPCDialogueTrigger>(FindObjectsSortMode.None);

        for (int i = 0; i < npcs.Length; i++)
        {
            for (int j = 0; j < savedProgress.Count; j++)
            {
                if (npcs[i].SaveId == savedProgress[j].saveId)
                {
                    npcs[i].RestoreProgress(
                        savedProgress[j].nextConversationIndex,
                        savedProgress[j].hasTalked
                    );
                    break;
                }
            }
        }
    }

    private void SaveDialogueHistoryIfEnabled()
    {
        if (saveHistoryAutomatically)
        {
            SaveDialogueHistory();
        }
    }

    void OnApplicationQuit()
    {
        SaveDialogueHistoryIfEnabled();
    }

    void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            SaveDialogueHistoryIfEnabled();
        }
    }

    private void ApplyChineseFont()
    {
        if (chineseFontAsset == null)
        {
            return;
        }

        SetFont(tmpSpeakerText);
        SetFont(tmpDialogueText);
        SetFont(tmpHistoryText);

        if (historyController != null)
        {
            SetFont(historyController.tmpHistoryText);
        }

        SetButtonFont(historyButton);
        SetButtonFont(autoButton);
        SetButtonFont(skipButton);
    }

    private void SetFont(TMP_Text text)
    {
        if (text != null)
        {
            text.font = chineseFontAsset;
        }
    }

    private void SetButtonFont(Button button)
    {
        if (button == null)
        {
            return;
        }

        SetFont(button.GetComponentInChildren<TMP_Text>());
    }

    private bool WasAdvancePressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null &&
            (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
        {
            return true;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            return !IsPointerOverProtectedUI();
        }
#endif

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            return true;
        }

        return Input.GetMouseButtonDown(0) && !IsPointerOverProtectedUI();
    }

    private bool IsPointerOverProtectedUI()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = GetMousePosition()
        };
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        for (int i = 0; i < raycastResults.Count; i++)
        {
            GameObject hitObject = raycastResults[i].gameObject;

            if (hitObject.GetComponent<Button>() != null)
            {
                return true;
            }

            if (historyPanel != null && hitObject.transform.IsChildOf(historyPanel))
            {
                return true;
            }
        }

        return false;
    }

    private Vector2 GetMousePosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
#endif

        return Input.mousePosition;
    }

    private void SetHoverEffectsPaused(bool paused)
    {
        if (hoverEffects == null)
        {
            hoverEffects = FindObjectsByType<HoverEffect>(FindObjectsSortMode.None);
        }

        for (int i = 0; i < hoverEffects.Length; i++)
        {
            if (hoverEffects[i] != null)
            {
                hoverEffects[i].SetPaused(paused);
            }
        }
    }

    private bool WasHistoryTogglePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.F1);
#endif
    }

    private bool WasHistoryClosePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}

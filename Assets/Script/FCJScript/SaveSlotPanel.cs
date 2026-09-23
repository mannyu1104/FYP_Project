using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Slots are immutable snapshots of the working save files; autosaves never touch them.
public class SaveSlotPanel : MonoBehaviour
{
    public const int SlotCount = 6;
    [Serializable] public class SaveFile { public string name, json; }
    [Serializable] public class Snapshot
    {
        public int version = 1, chapter = 1, location, area, part;
        public string savedUtc, scene, checksum;
        public float lookX;
        public InvestigationFlowController.Stage stage;
        public DialogueController.ConversationSnapshot conversation;
        public List<SaveFile> files = new List<SaveFile>();
    }
    private static readonly string[] FileNames = { "inventory.json", "inventorylock.json", "map.json",
        "dialogue_history.json", "clues.json", "story_progress.json", "notes.json", "welfare_progress.json", "whiteboard.json" };
    private static Snapshot pending;
    private static Snapshot[] slots;
    private static SaveSlotPanel instance;
    public static bool HasPendingLoad => pending != null;
    public static bool IsOpen => instance != null && instance.gameObject.activeSelf;
    public static bool HasSaves { get { if (slots == null) Refresh(); return Array.Exists(slots, s => s != null); } }
    private MainMenuController menu;
    private bool saving, wasPaused;
    private int confirmation = -1;
    private TMP_Text title, message;
    private readonly List<Button> cards = new List<Button>();
    private Button confirmButton, backButton;
    private static bool Chinese => LocalizationSettings.SelectedLocale != null &&
        LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("zh", StringComparison.OrdinalIgnoreCase);
    private static string L(string zh, string en) => Chinese ? zh : en;
    private static string SlotPath(int index) => Path.Combine(Application.persistentDataPath, "SaveSlots", "slot_" + (index + 1) + ".json");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { pending = null; slots = null; instance = null; }

    private static Snapshot Read(int index)
    {
        try
        {
            string path = SlotPath(index);
            if (!File.Exists(path)) return null;
            var data = JsonUtility.FromJson<Snapshot>(File.ReadAllText(path));
            if (data == null || data.version != 1 || data.chapter < 1 || string.IsNullOrEmpty(data.scene) ||
                !DateTime.TryParse(data.savedUtc, out _) || data.files == null ||
                !Enum.IsDefined(typeof(InvestigationFlowController.Stage), data.stage) ||
                data.stage == InvestigationFlowController.Stage.Menu) return null;
            if (string.IsNullOrEmpty(data.checksum) || data.checksum != Digest(data)) return null;
            foreach (string required in FileNames)
            {
                if (required == "notes.json" || required == "welfare_progress.json" || required == "whiteboard.json") continue;
                var file = data.files.Find(f => f != null && f.name == required);
                if (file == null || string.IsNullOrEmpty(file.json) || !file.json.TrimStart().StartsWith("{")) return null;
            }
            // Validate payloads before enabling a load button.
            if (JsonUtility.FromJson<InventorySaveData>(data.files.Find(f => f.name == "inventory.json").json)?.ItemBool == null ||
                JsonUtility.FromJson<ItemLockSaveData>(data.files.Find(f => f.name == "inventorylock.json").json)?.ItemLockBool == null ||
                JsonUtility.FromJson<MapSaveData>(data.files.Find(f => f.name == "map.json").json)?.MapBool == null) return null;
            return data;
        }
        catch (Exception) { return null; }
    }
    private static string Digest(Snapshot data)
    {
        string checksum = data.checksum;
        data.checksum = null;
        string json = JsonUtility.ToJson(data);
        data.checksum = checksum;
        using (var sha = System.Security.Cryptography.SHA256.Create())
            return Convert.ToBase64String(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(json)));
    }
    private static void Refresh()
    {
        slots = new Snapshot[SlotCount];
        for (int i = 0; i < SlotCount; i++) slots[i] = Read(i);
    }

    public static void Open(MainMenuController owner, bool save)
    {
        if (HasPendingLoad || (save && !owner.IsGameplayVisible)) return;
        if (instance == null)
        {
            var root = new GameObject("Save & Load", typeof(RectTransform));
            instance = root.AddComponent<SaveSlotPanel>();
            instance.Build();
        }
        instance.menu = owner;
        instance.saving = save;
        instance.confirmation = -1;
        var look = FindAnyObjectByType<LookController>();
        if (!IsOpen || instance.title.text.Length == 0) instance.wasPaused = look != null && look.IsPaused;
        look?.SetPaused(true);
        Refresh();
        instance.gameObject.SetActive(true);
        instance.Render();
    }
    private void Close()
    {
        gameObject.SetActive(false);
        FindAnyObjectByType<LookController>()?.SetPaused(wasPaused);
    }
    private void Render()
    {
        backButton.GetComponentInChildren<TMP_Text>().text = L("\u8fd4\u56de", "Back");
        title.text = saving ? L("保存游戏", "SAVE GAME") : L("读取游戏", "LOAD GAME");
        message.text = saving ? L("选择一个存档位置", "Choose a save slot") : L("选择要继续的存档", "Choose a save to continue");
        confirmButton.gameObject.SetActive(false);
        for (int i = 0; i < SlotCount; i++)
        {
            Snapshot data = slots[i];
            cards[i].interactable = saving || data != null;
            string label = L("存档 ", "SLOT ") + (i + 1).ToString("00");
            label += data == null ? "\n\n" + (File.Exists(SlotPath(i)) ? L("存档损坏", "Unreadable save") : L("空存档", "Empty slot")) :
                "\n\nChapter " + data.chapter + " · " + StageName(data.stage) + "\n" +
                DateTime.Parse(data.savedUtc).ToLocalTime().ToString("yyyy-MM-dd  HH:mm:ss");
            cards[i].GetComponentInChildren<TMP_Text>().text = label;
        }
    }
    private static string StageName(InvestigationFlowController.Stage stage)
    {
        int n = (int)stage;
        string[] zh = { "菜单", "开场", "等待测验", "教程测验", "测验结束", "入睡", "翌日早晨", "等待新闻", "阅读新闻", "敲门前", "等待开门", "委托人来访", "自由调查" };
        string[] en = { "Menu", "Introduction", "Before tutorial", "Tutorial", "After tutorial", "Sleeping", "Morning", "Before news", "Reading news", "After news", "At the door", "Client visit", "Investigation" };
        return (Chinese ? zh : en)[Mathf.Clamp(n, 0, zh.Length - 1)];
    }
    private void Select(int index)
    {
        if (saving && File.Exists(SlotPath(index)))
        {
            confirmation = index;
            message.text = L("覆盖存档 ", "Overwrite slot ") + (index + 1) + "?";
            confirmButton.GetComponentInChildren<TMP_Text>().text = L("确认覆盖", "Overwrite");
            confirmButton.gameObject.SetActive(true);
        }
        else if (!saving && menu.IsGameplayVisible)
        {
            confirmation = index;
            message.text = L("读取后将放弃当前未保存进度。", "Loading replaces your unsaved progress.");
            confirmButton.GetComponentInChildren<TMP_Text>().text = L("确认读取", "Load");
            confirmButton.gameObject.SetActive(true);
        }
        else Execute(index);
    }
    private void Execute(int index)
    {
        try
        {
            if (saving)
            {
                menu.CaptureSaveFiles();
                var flow = InvestigationFlowController.Instance;
                var data = new Snapshot { savedUtc = DateTime.UtcNow.ToString("O"), scene = menu.gameObject.scene.path,
                    stage = flow.CurrentStage, part = flow.CurrentStage == InvestigationFlowController.Stage.Tutorial && flow.TutorialScored ? -1 : flow.ConversationPart,
                    conversation = FindAnyObjectByType<DialogueController>(FindObjectsInactive.Include)?.CaptureConversation(),
                    location = FindAnyObjectByType<MapPanelNavigator>(FindObjectsInactive.Include)?.SavedLocation ?? 0,
                    area = FindAnyObjectByType<LocationNavigator>(FindObjectsInactive.Include)?.SavedArea ?? 0 };
                foreach (string name in FileNames)
                {
                    string path = Path.Combine(Application.persistentDataPath, name);
                    if (File.Exists(path)) data.files.Add(new SaveFile { name = name, json = File.ReadAllText(path) });
                }
                data.lookX = FindAnyObjectByType<LookController>(FindObjectsInactive.Include)?.SavedLookX ?? 0;
                data.checksum = Digest(data);
                string target = SlotPath(index);
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                File.WriteAllText(target + ".tmp", JsonUtility.ToJson(data, true));
                if (File.Exists(target)) File.Replace(target + ".tmp", target, null);
                else File.Move(target + ".tmp", target);
                Refresh(); Render();
                message.text = L("已保存到存档 ", "Saved to slot ") + (index + 1);
            }
            else
            {
                Snapshot data = Read(index);
                if (data == null) throw new IOException("Invalid save slot");
#if !UNITY_EDITOR
                if (!Application.CanStreamedLevelBeLoaded(data.scene)) throw new IOException("Scene is unavailable");
#endif
                pending = data;
#if UNITY_EDITOR
                UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(data.scene, new LoadSceneParameters(LoadSceneMode.Single));
#else
                SceneManager.LoadScene(data.scene);
#endif
            }
        }
        catch (Exception ex)
        {
            pending = null;
            message.text = L("操作失败，原有存档未被删除。", "Operation failed. Existing saves were not deleted.");
            Debug.LogException(ex);
        }
        confirmation = -1;
        if (confirmButton != null) confirmButton.gameObject.SetActive(false);
    }
    public static void WritePendingFiles()
    {
        if (pending == null) return;
        foreach (string name in FileNames)
        {
            SaveFile file = pending.files.Find(f => f.name == name);
            string path = Path.Combine(Application.persistentDataPath, name);
            if (file != null) File.WriteAllText(path, file.json);
            else if (File.Exists(path)) File.Delete(path);
        }
    }
    public static void RestorePendingState()
    {
        var data = pending;
        if (data == null) return;
        FindAnyObjectByType<MapPanelNavigator>(FindObjectsInactive.Include)?.RestoreLocation(data.location);
        if (data.location == 1) FindAnyObjectByType<LocationNavigator>(FindObjectsInactive.Include)?.RestoreArea(data.area);
        FindAnyObjectByType<LookController>(FindObjectsInactive.Include)?.RestoreLookX(data.lookX);
        WelfareInteractionController.Instance?.LoadProgress();
        foreach (var board in FindObjectsByType<WhiteBoardSurface>(FindObjectsInactive.Include)) board.LoadLayout();
        pending = null;
        if (InvestigationFlowController.Instance != null) InvestigationFlowController.Instance.TutorialScored = data.stage == InvestigationFlowController.Stage.Tutorial && data.part == -1;
        InvestigationFlowController.Instance?.RestoreStage(data.stage, Mathf.Max(0, data.part), data.conversation);
        WelfareInteractionController.Instance?.ResumeSavedInteraction();
    }
    private void Build()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 31000;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();
        var shade = Rect("Backdrop", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        shade.gameObject.AddComponent<Image>().color = new Color(.94f, .97f, 1f, 1f);
        title = Label(shade, "", 36); title.rectTransform.anchorMin = new Vector2(.07f, .84f); title.rectTransform.anchorMax = new Vector2(.75f, .96f);
        for (int i = 0; i < SlotCount; i++)
        {
            int slot = i, col = i % 3, row = i / 3;
            var rect = Rect("Slot " + (i + 1), shade, new Vector2(.07f + col * .295f, .49f - row * .29f),
                new Vector2(.345f + col * .295f, .75f - row * .29f), Vector2.zero, Vector2.zero);
            var button = MakeButton(rect, "", () => Select(slot)); cards.Add(button);
        }
        message = Label(shade, "", 22); message.rectTransform.anchorMin = new Vector2(.07f, .07f); message.rectTransform.anchorMax = new Vector2(.65f, .17f);
        confirmButton = MakeButton(Rect("Confirm", shade, new Vector2(.67f,.07f),new Vector2(.81f,.16f),Vector2.zero,Vector2.zero), "", () => { if (confirmation >= 0) Execute(confirmation); });
        backButton = MakeButton(Rect("Back", shade,new Vector2(.83f,.07f),new Vector2(.94f,.16f),Vector2.zero,Vector2.zero), L("返回", "Back"), Close);
    }
    private static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max, Vector2 offMin, Vector2 offMax)
    {
        var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform; rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = offMin; rect.offsetMax = offMax; return rect;
    }
    private static TMP_Text Label(Transform parent, string text, float size)
    {
        var rect = Rect("Label", parent, Vector2.zero, Vector2.one, new Vector2(18, 8), new Vector2(-18, -8));
        var label = rect.gameObject.AddComponent<TextMeshProUGUI>(); label.text = text; label.fontSize = size;
        label.color = new Color(.12f,.2f,.27f); label.alignment = TextAlignmentOptions.MidlineLeft;
        label.enableAutoSizing = true; label.fontSizeMin = 14; label.fontSizeMax = size; label.raycastTarget = false;
        LocalizedFontController.Instance?.ApplyTo(label);
        return label;
    }
    private static Button MakeButton(RectTransform rect, string text, UnityEngine.Events.UnityAction action)
    {
        var image = rect.gameObject.AddComponent<Image>(); image.color = new Color(.76f,.89f,.98f);
        var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
        var colors = button.colors; colors.highlightedColor = new Color(.75f,.88f,1); colors.disabledColor = new Color(.78f,.81f,.84f); button.colors = colors;
        button.onClick.AddListener(action); Label(rect, text, 24); return button;
    }
}

using System;
using System.Collections;
using UnityEngine.Localization;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

// Scene-owned child encounters. Game assignment is shuffled once, then stored in each save slot.
public class WelfareInteractionController : MonoBehaviour
{
    [Serializable] public class ChildProgress
    {
        public bool rope, finished, unlocked;
        public int rounds, mask;
    }
    public enum InteractionPhase { None, Invitation, Choices, PlayLater, Result, Candy, BagFull }
    [Serializable] public class Progress
    {
        public bool parkKeyCollected;
        public InteractionPhase interaction;
        public int interactionIndex;
        public ChildProgress[] children = new ChildProgress[3];
        public bool[] collected = new bool[3], consumed = new bool[3];
    }
    public static WelfareInteractionController Instance { get; private set; }
    public static bool IsOpen => Instance != null && (Instance.interactionActive || Instance.modal != null || Instance.match != null);
    public static bool BlocksDialogue => Instance != null && (Instance.modal != null || Instance.match != null);
    public static bool BlocksSettings => BlocksDialogue || (Instance != null && Instance.interactionActive &&
        Instance.data.interaction != InteractionPhase.Candy && Instance.data.interaction != InteractionPhase.BagFull);
    private bool interactionActive;
    private DialogueController Dialogue => FindAnyObjectByType<DialogueController>();
    private static LocalizedString TextKey(string key) => new LocalizedString("Script Assets", key);
    // The rope scene's adapter registers a launcher and calls its completion callback exactly once.
    public Action<Action<bool>> RopeLauncher { get; set; }
    private Progress data;
    private readonly NPCDialogueTrigger[] children = new NPCDialogueTrigger[3];
    [SerializeField] private Image[] candyPlaceholders = new Image[3]; // Home, park, staff office.
    private readonly GameObject[] pickups = new GameObject[3];
    private readonly DragableItem[] candy = new DragableItem[3];
    private InventoryManager inventory;
    private NPCDialogueTrigger parkElder;
    private CursorInteractionTarget parkKey;
    private DragableItem keyItem;
    private GameObject modal, match;
    private readonly List<Behaviour> suspendedForRope = new List<Behaviour>();

    private void RestoreRopeView()
    {
        foreach (var component in suspendedForRope)
            if (component != null) component.enabled = true;
        suspendedForRope.Clear();
    }

    private void StartRope(int index)
    {
        RopeSkipping template = null;
        foreach (var candidate in FindObjectsByType<RopeSkipping>(FindObjectsInactive.Include))
            if (!candidate.gameObject.activeInHierarchy && candidate.gameObject.scene == gameObject.scene)
            { template = candidate; break; }
        // Prefer the editable scene instance; the variant references the same original
        // prefab and remains available when the scene instance is removed or replaced.
        GameObject source = template != null ? template.transform.root.gameObject : Resources.Load<GameObject>("WelfareRopeGame");
        if (source == null || source.GetComponentInChildren<RopeSkipping>(true) == null)
        {
            Debug.LogError("Rope game prefab is missing or has no RopeSkipping component.", this);
            StartCoroutine(PlayLater(index));
            return;
        }
        match = Instantiate(source);
        match.name = "Child Rope Match";
        match.transform.position += new Vector3(1000, 0, 0);
        foreach (var events in match.GetComponentsInChildren<UnityEngine.EventSystems.EventSystem>(true))
            events.gameObject.SetActive(false);
        foreach (var camera in FindObjectsByType<Camera>())
            if (camera.enabled) { suspendedForRope.Add(camera); camera.enabled = false; }
        foreach (var listener in FindObjectsByType<AudioListener>())
            if (listener.enabled) { suspendedForRope.Add(listener); listener.enabled = false; }
        foreach (var canvas in FindObjectsByType<Canvas>())
            if (canvas.enabled) { suspendedForRope.Add(canvas); canvas.enabled = false; }
        var game = match.GetComponentInChildren<RopeSkipping>(true);
        game.MatchFinished += won =>
        {
            if (this == null || match == null) return;
            match.SetActive(false);
            RestoreRopeView();
            Finish(index, won);
        };
        match.SetActive(true);
    }
    private bool allowDialogue, lookWasPaused;
    private bool lastChinese;
    private static bool Chinese
    {
        get
        {
            try
            {
                if (!LocalizationSettings.HasSettings) return false;
                var initialization = LocalizationSettings.InitializationOperation;
                if (!initialization.IsDone) return false;
                var selected = LocalizationSettings.SelectedLocaleAsync;
                if (!selected.IsDone) return false;
                var locale = selected.Result;
                return locale != null && !string.IsNullOrEmpty(locale.Identifier.Code) &&
                       locale.Identifier.Code.StartsWith("zh", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
    private static string L(string zh, string en) => Chinese ? zh : en;
    private string PathName => Path.Combine(Application.persistentDataPath, "welfare_progress.json");

    private void Awake() { Instance = this; ResetProgress(); }
    private void Start()
    {
        foreach (var npc in FindObjectsByType<NPCDialogueTrigger>(FindObjectsInactive.Include))
            for (int i = 0; i < 3; i++) if (npc.name == "孩子" + (char)('A' + i)) children[i] = npc;
        foreach (var candidate in FindObjectsByType<InventoryManager>(FindObjectsInactive.Include))
            if (candidate.inventorySlotsForIngame != null && candidate.inventorySlotsForIngame.Length > 0) { inventory = candidate; break; }
        var map = FindAnyObjectByType<MapPanelNavigator>(FindObjectsInactive.Include);
        var locations = FindAnyObjectByType<LocationNavigator>(FindObjectsInactive.Include);
        CreateCandy(0, map != null ? map.LocationRoot(0) : null, new Vector2(.72f, .35f));
        CreateCandy(1, map != null ? map.LocationRoot(3) : null, new Vector2(.65f, .25f));
        CreateCandy(2, locations != null ? locations.StaffRoomRoot : null, new Vector2(.6f, .4f));
        foreach (var npc in FindObjectsByType<NPCDialogueTrigger>(FindObjectsInactive.Include))
            if (npc.name == "\u8001\u7237\u7237") parkElder = npc;
        foreach (var target in FindObjectsByType<CursorInteractionTarget>(FindObjectsInactive.Include))
            if ((target.name == "\u94a5\u5319" || target.name == "\u94a5\u5319\uff08\u65e0\u56fe\uff09") &&
                map != null && map.LocationRoot(3) != null && target.transform.IsChildOf(map.LocationRoot(3).transform))
                parkKey = target;
        if (parkKey != null)
        {
            parkKey.enableInspectDialogue = false;
            var button = parkKey.GetComponent<Button>();
            if (button == null) button = parkKey.gameObject.AddComponent<Button>();
            button.onClick.RemoveListener(PickUpParkKey);
            button.onClick.AddListener(PickUpParkKey);
            var obj = new GameObject("Park key inventory", typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(transform, false);
            var icon = obj.GetComponent<Image>(); icon.sprite = parkKey.GetComponent<Image>().sprite;
            var label = Label(obj.transform, "", Vector2.zero, 20);
            keyItem = obj.AddComponent<DragableItem>();
            keyItem.ConfigureCandy(9103, label, icon);
            obj.SetActive(false);
            parkKey.gameObject.SetActive(false);
        }

    }
    private void OnDestroy()
    {
        if (match != null) { match.SetActive(false); Destroy(match); }
        RestoreRopeView();
        if (Instance == this) Instance = null;
    }
    private void ResetProgress()
    {
        data = new Progress();
        bool[] assignments = { false, true, UnityEngine.Random.value < .5f };
        for (int i = 2; i > 0; i--) { int j = UnityEngine.Random.Range(0, i + 1); bool v = assignments[i]; assignments[i] = assignments[j]; assignments[j] = v; }
        for (int i = 0; i < 3; i++) data.children[i] = new ChildProgress { rope = assignments[i] };
    }
    private bool CanInteract()
    {
        var flow = InvestigationFlowController.Instance;
        var menu = FindAnyObjectByType<MainMenuController>();
        var dialogue = FindAnyObjectByType<DialogueController>();
        return flow != null && flow.CurrentStage == InvestigationFlowController.Stage.Investigation &&
            !WhiteBoard.IsAnyWhiteBoardOpen && menu != null && menu.IsGameplayVisible && !menu.IsSettingsVisible && !SaveSlotPanel.IsOpen &&
            (dialogue == null || !dialogue.IsDialogueActive);
    }
    public bool IsChild(NPCDialogueTrigger npc) => npc != null && Array.IndexOf(children, npc) >= 0;

    public bool TryInteract(NPCDialogueTrigger npc)
    {
        int index = Array.IndexOf(children, npc);
        if (index < 0 || allowDialogue) return false;
        if (IsOpen || !CanInteract()) return true;
        if (data.children[index].unlocked) { TellClue(index); return true; }
        ShowChild(index);
        return true;
    }
    private void BeginInteraction()
    {
        if (interactionActive) return;
        var look = FindAnyObjectByType<LookController>();
        lookWasPaused = look != null && look.IsPaused;
        look?.SetPaused(true); interactionActive = true;
    }
    private IEnumerator Say(string key, int child = -1)
    {
        yield return LocalizationSettings.InitializationOperation;
        var dialogue = Dialogue;
        if (dialogue == null) yield break;
        LocalizedString speaker = TextKey("Character_010");
        if (child >= 0 && children[child] != null && children[child].conversations.Count > 0 &&
            children[child].conversations[0].dialogueLines.Count > 0)
            speaker = children[child].conversations[0].dialogueLines[0].speakerName;
        dialogue.StartConversation(new List<DialogueController.DialogueLine> {
            new DialogueController.DialogueLine { speakerName = speaker, dialogueText = TextKey(key) }
        }, null);
        while (dialogue.IsDialogueActive) yield return null;
    }
    private void ShowChild(int index)
    {
        BeginInteraction(); StartCoroutine(ChildInvitation(index));
    }
    private IEnumerator ChildInvitation(int index)
    {
        data.interaction = InteractionPhase.Invitation; data.interactionIndex = index;
        yield return Say(data.children[index].finished ? "Child_CandyRequest" : "Child_PlayInvite", index);
        ShowChildChoices(index);
    }
    private void ShowChildChoices(int index)
    {
        data.interaction = InteractionPhase.Choices; data.interactionIndex = index;
        // End the interaction so the player can open the inventory and drag candy.
        if (data.children[index].finished) { ClosePanel(); return; }
        Dialogue.ShowInteractionChoices(
            new[] { data.children[index].rope ? L("\u8df3\u7ef3", "Jump rope") : L("\u526a\u5200\u77f3\u5934\u5e03", "Rock, paper, scissors"), L("\u6682\u65f6\u79bb\u5f00", "Leave") },
            new Action[] { () => StartGame(index), ClosePanel });
    }

    private IEnumerator PlayLater(int index)
    {
        data.interaction = InteractionPhase.PlayLater; data.interactionIndex = index;
        yield return Say("Child_PlayLater", index); ClosePanel();
    }
    private void StartGame(int index)
    {
        var state = data.children[index];
        if (state.finished) return;
        if (state.rope)
        {
            StartCoroutine(ExplainAndStartRope(index));
            return;
        }
        var prefab = Resources.Load<GameObject>("WelfareRockPaperScissors");
        if (prefab == null) { Debug.LogError("Welfare RPS prefab missing"); return; }
        ClearPanelContents();
        match = Instantiate(prefab);
        var game = match.GetComponentInChildren<ScissorPaperStone>(true);
        int wins = 0; for (int i = 0; i < state.rounds; i++) if ((state.mask & (1 << i)) != 0) wins++;
        game.RestoreMatch(state.rounds, wins, state.mask);
        game.RoundResolved += (rounds, mask) => { state.rounds = rounds; state.mask = mask; };
        game.MatchFinished += won => Finish(index, won);
        Label(match.transform, L("三局两胜 · 平局重来", "Best of three · Draws replay"), new Vector2(0, 290), 26);
        Button(match.transform, L("返回（保留进度）", "Back (keep progress)"), new Vector2(380, -290), () =>
        {
            if (game.playing) return; // A chosen round must resolve before leaving.
            Destroy(match); match = null; ClosePanel();
        });
    }
    private IEnumerator ExplainAndStartRope(int index)
    {
        yield return Say("Child_RopeControls", index);
        if (!interactionActive || data.children[index].finished) yield break;
        LaunchRope(index);
    }

    private void LaunchRope(int index)
    {
        if (RopeLauncher == null) { StartRope(index); return; }
        ClearPanelContents();
        bool answered = false;
        RopeLauncher(won => { if (answered || this == null) return; answered = true; Finish(index, won); });
    }

    private void Finish(int index, bool won)
    {
        var state = data.children[index];
        if (state.finished) return;
        state.finished = true; state.unlocked = won;
        if (match != null) Destroy(match); match = null;
        ClosePanel(); BeginInteraction();
        StartCoroutine(ExplainResult(index, won));
    }
    private IEnumerator ExplainResult(int index, bool won)
    {
        data.interaction = InteractionPhase.Result; data.interactionIndex = index;
        yield return Say(won ? "Child_GameWon" : "Child_GameLost", index);
        if (won) { ClosePanel(); TellClue(index); }
        else ShowChildChoices(index);
    }
    public bool TryGiveCandy(DragableItem item, Vector2 screenPosition)
    {
        if (item == null || IsOpen || !CanInteract()) return false;
        int id = Array.IndexOf(candy, item);
        if (id < 0 || !data.collected[id] || data.consumed[id] || !item.thisGet || item.thisUsed)
            return false;
        for (int index = 0; index < children.Length; index++)
        {
            var child = children[index];
            if (child == null || !child.gameObject.activeInHierarchy ||
                !data.children[index].finished || data.children[index].unlocked) continue;
            bool visible = true;
            foreach (var group in child.GetComponentsInParent<CanvasGroup>())
                if (group.alpha <= .01f) { visible = false; break; }
            if (!visible) continue;
            var rect = child.transform as RectTransform;
            var canvas = child.GetComponentInParent<Canvas>();
            var camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera : null;
            if (rect == null || !RectTransformUtility.RectangleContainsScreenPoint(rect, screenPosition, camera))
                continue;

            // Consume the exact candy that was dropped, once, and free its inventory slot.
            data.consumed[id] = true;
            data.children[index].unlocked = true;
            item.thisGet = false;
            item.thisUsed = true;
            item.isdragging = false;
            item.parentAfterDrag = transform;
            item.transform.SetParent(transform, false);
            item.gameObject.SetActive(false);
            foreach (var inventoryPanel in FindObjectsByType<OpenCanvasButton>(FindObjectsInactive.Include))
                inventoryPanel.CloseAll();
            TellClue(index);
            return true;
        }
        return false;
    }
    private void TellClue(int index)
    {
        if (children[index] == null) return;
        allowDialogue = true;
        try { children[index].StartDialogue(); } finally { allowDialogue = false; }
    }
    private void CreateCandy(int id, GameObject parent, Vector2 anchor)
    {
        if (parent == null) { Debug.LogError("Candy location missing: " + id); return; }
        Image placeholder = candyPlaceholders != null && id < candyPlaceholders.Length ? candyPlaceholders[id] : null;
        foreach (var candidate in parent.GetComponentsInChildren<Image>(true))
            if (placeholder == null && candidate.name.Contains("\u7cd6\u679c")) { placeholder = candidate; break; }
        if (placeholder == null) { Debug.LogWarning("Candy image placeholder missing in " + parent.name, parent); return; }
        pickups[id] = placeholder.gameObject;
        var pickup = placeholder.GetComponent<Button>();
        if (pickup == null) pickup = placeholder.gameObject.AddComponent<Button>();
        pickup.onClick.AddListener(() => { if (!IsOpen && CanInteract()) InspectCandy(id); });
        var target = placeholder.GetComponent<CursorInteractionTarget>();
        if (target == null) target = placeholder.gameObject.AddComponent<CursorInteractionTarget>();
        target.cursorPresetName = "View";
        target.enableInspectDialogue = false;
        target.itemName = TextKey("Candy_Name");
        target.itemDescriptionLines = new List<LocalizedString> { TextKey("Candy_Description") };
        target.showInspectImage = false;
        var itemObject = new GameObject("Candy_" + id, typeof(RectTransform), typeof(Image));
        itemObject.transform.SetParent(transform, false);
        int itemLayer = LayerMask.NameToLayer("Item");
        if (itemLayer >= 0) itemObject.layer = itemLayer;
        var image = itemObject.GetComponent<Image>(); image.sprite = placeholder.sprite; image.color = placeholder.color; image.preserveAspect = true;
        var text = Label(itemObject.transform, L("糖果", "Candy"), Vector2.zero, 20);
        text.rectTransform.sizeDelta = new Vector2(90, 38);
        text.gameObject.SetActive(placeholder.sprite == null);
        candy[id] = itemObject.AddComponent<DragableItem>(); candy[id].ConfigureCandy(9100 + id, text, image);
        candy[id].thisName = L("\u7cd6\u679c", "Candy");
        candy[id].thisDescription = L("\u8fd9\u662f\u4e00\u9897\u7cd6\u679c\u3002\u5982\u679c\u6211\u662f\u5c0f\u5b69\u5b50\uff0c\u5e94\u8be5\u4f1a\u5f88\u559c\u6b22\u5427\u3002", "A piece of candy. If I were a child, I would probably love it.");
        itemObject.SetActive(false);
    }
    public void InspectCandy(int id)
    {
        if (IsOpen || !CanInteract() || data.consumed[id]) return;
        BeginInteraction(); StartCoroutine(InspectAndCollect(id));
    }
    private IEnumerator InspectAndCollect(int id)
    {
        data.interaction = InteractionPhase.Candy; data.interactionIndex = id;
        yield return LocalizationSettings.InitializationOperation;
        var dialogue = Dialogue;
        if (dialogue == null) { ClosePanel(); yield break; }
        dialogue.ShowItemInspect(TextKey("Candy_Name"), new[] { TextKey("Candy_Description") }, null, false);
        while (dialogue.IsDialogueActive) yield return null;
        if (!data.collected[id]) PickUp(id);
        else ClosePanel();
    }
    private IEnumerator BagFull()
    {
        data.interaction = InteractionPhase.BagFull;
        yield return Say("Candy_BagFull"); ClosePanel();
    }
    private void PickUp(int id)
    {
        if (inventory == null || candy[id] == null || data.collected[id]) return;
        InventorySlot free = null;
        foreach (var slot in inventory.inventorySlotsForIngame) if (slot != null && slot.transform.childCount == 0) { free = slot; break; }
        if (free == null) { StartCoroutine(BagFull()); return; }
        data.collected[id] = true;
        candy[id].gameObject.SetActive(true); inventory.AddItem(candy[id].gameObject);
        var rect = (RectTransform)candy[id].transform; rect.anchoredPosition = Vector2.zero; rect.sizeDelta = new Vector2(90, 42); rect.localScale = Vector3.one;
        pickups[id].SetActive(false); ClosePanel();
    }
    private static void CandyWrapper(Transform parent, float x)
    {
        var obj = new GameObject("Wrapper", typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        var rect = (RectTransform)obj.transform;
        rect.sizeDelta = new Vector2(24, 24); rect.anchoredPosition = new Vector2(x, 0);
        rect.localRotation = Quaternion.Euler(0, 0, 45);
        obj.GetComponent<Image>().color = new Color(1, .75f, .83f);
        obj.GetComponent<Image>().raycastTarget = false;
    }

    private void Update()
    {
        if (lastChinese != Chinese)
        {
            lastChinese = Chinese;
            for (int i = 0; i < 3; i++)
            {

                if (candy[i] != null) candy[i].GetComponentInChildren<TMP_Text>(true).text = L("\u7cd6\u679c", "Candy");
            }
        }
        bool show = InvestigationFlowController.Instance != null && InvestigationFlowController.Instance.CurrentStage == InvestigationFlowController.Stage.Investigation;
        for (int i = 0; i < 3; i++) if (pickups[i] != null) pickups[i].SetActive(show && !data.collected[i]);
        if (parkKey != null) parkKey.gameObject.SetActive(show && parkElder != null && parkElder.HasTalked && !data.parkKeyCollected);
    }
    private void PickUpParkKey()
    {
        if (!CanInteract() || IsOpen || parkElder == null || !parkElder.HasTalked || data.parkKeyCollected || inventory == null || keyItem == null) return;
        keyItem.gameObject.SetActive(true);
        inventory.AddItem(keyItem.gameObject);
        if (!keyItem.thisGet) { keyItem.gameObject.SetActive(false); return; }
        data.parkKeyCollected = true;
        ((RectTransform)keyItem.transform).sizeDelta = new Vector2(90, 90);
        keyItem.transform.localScale = Vector3.one;
        parkKey.gameObject.SetActive(false);
    }

    public void InspectParkKey()
    {
        if (!CanInteract() || IsOpen || parkKey == null) return;
        Dialogue.ShowItemInspect(parkKey.itemName, parkKey.itemDescriptionLines, parkKey.GetComponent<Image>().sprite, true);
    }

    public void SaveProgress() => File.WriteAllText(PathName, JsonUtility.ToJson(data));
    public void LoadProgress()
    {
        if (File.Exists(PathName))
        {
            var saved = JsonUtility.FromJson<Progress>(File.ReadAllText(PathName));
            if (saved != null && saved.children != null && saved.children.Length == 3 && saved.collected?.Length == 3 && saved.consumed?.Length == 3) data = saved;
        }
        if (keyItem != null)
        {
            keyItem.thisGet = data.parkKeyCollected;
            keyItem.gameObject.SetActive(data.parkKeyCollected);
            if (data.parkKeyCollected)
            {
                inventory?.AddItem(keyItem.gameObject);
                ((RectTransform)keyItem.transform).sizeDelta = new Vector2(90, 90);
                keyItem.transform.localScale = Vector3.one;
            }
        }
        for (int i = 0; i < 3; i++)
        {
            if (candy[i] == null) continue;
            candy[i].thisGet = data.collected[i] && !data.consumed[i]; candy[i].thisUsed = data.consumed[i];
            candy[i].gameObject.SetActive(candy[i].thisGet);
            if (candy[i].thisGet)
            {
                inventory?.AddItem(candy[i].gameObject);
                var rect = (RectTransform)candy[i].transform;
                rect.anchoredPosition = Vector2.zero; rect.sizeDelta = new Vector2(90, 42); rect.localScale = Vector3.one;
            }
            else candy[i].transform.SetParent(transform);
        }
    }
    public void StopForMenu()
    {
        StopAllCoroutines();
        ClosePanel();
    }

    // Called after the slot restores the currently spoken line. Continue its side effects once.
    public void ResumeSavedInteraction()
    {
        if (data.interaction == InteractionPhase.None) return;
        BeginInteraction();
        // The restored conversation may already have paused the camera.
        lookWasPaused = false;
        StartCoroutine(ResumeInteraction());
    }

    private IEnumerator ResumeInteraction()
    {
        var phase = data.interaction;
        int index = data.interactionIndex;
        if (index < 0 || index >= 3) { ClosePanel(); yield break; }
        if (phase == InteractionPhase.Choices) { ShowChildChoices(index); yield break; }
        if (Dialogue == null || !Dialogue.IsDialogueActive)
        {
            switch (phase)
            {
                case InteractionPhase.Invitation: yield return ChildInvitation(index); break;
                case InteractionPhase.PlayLater: yield return PlayLater(index); break;
                case InteractionPhase.Result: yield return ExplainResult(index, data.children[index].unlocked); break;
                case InteractionPhase.Candy: yield return InspectAndCollect(index); break;
                case InteractionPhase.BagFull: yield return BagFull(); break;
            }
            yield break;
        }
        while (Dialogue != null && Dialogue.IsDialogueActive) yield return null;
        switch (phase)
        {
            case InteractionPhase.Invitation: ShowChildChoices(index); break;
            case InteractionPhase.Result:
                if (data.children[index].unlocked) { ClosePanel(); TellClue(index); }
                else ShowChildChoices(index);
                break;
            case InteractionPhase.Candy:
                if (!data.collected[index]) PickUp(index); else ClosePanel();
                break;
            default: ClosePanel(); break;
        }
    }

    private void OpenPanel()
    {
        if (modal != null) { ClearPanelContents(); return; }
        BeginInteraction();
        modal = new GameObject("Child interaction", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = modal.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 11000;
        var scaler = modal.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1280,720);
        var shade = new GameObject("Shade",typeof(RectTransform),typeof(Image)); shade.transform.SetParent(modal.transform,false);
        var rect = (RectTransform)shade.transform; rect.anchorMin=Vector2.zero; rect.anchorMax=Vector2.one; rect.offsetMin=rect.offsetMax=Vector2.zero;
        shade.GetComponent<Image>().color=new Color(.94f,.97f,1f,1f);
    }
    private void ClearPanelContents()
    {
        if (modal == null) { OpenPanel(); return; }
        foreach (Transform child in modal.transform) if (child.name != "Shade") { child.gameObject.SetActive(false); Destroy(child.gameObject); }
    }
    private void ClosePanel()
    {
        if (match != null) match.SetActive(false);
        RestoreRopeView();
        Dialogue?.ClearInteractionChoices();
        interactionActive = false;
        data.interaction = InteractionPhase.None;
        if (match != null) Destroy(match); match = null;
        if (modal != null) Destroy(modal); modal = null;
        FindAnyObjectByType<LookController>()?.SetPaused(lookWasPaused);
    }
    private static TMP_Text Label(Transform parent, string value, Vector2 position, int size)
    {
        var obj=new GameObject("Label",typeof(RectTransform),typeof(TextMeshProUGUI)); obj.transform.SetParent(parent,false);
        var label=obj.GetComponent<TextMeshProUGUI>(); label.text=value; label.fontSize=size; label.color=new Color(.12f,.2f,.27f); label.alignment=TextAlignmentOptions.Center; label.raycastTarget=false;
        label.rectTransform.sizeDelta=new Vector2(760,110); label.rectTransform.anchoredPosition=position;
        LocalizedFontController.Instance?.ApplyTo(label); return label;
    }
    private static Button Button(Transform parent,string title,Vector2 position,UnityEngine.Events.UnityAction click)
    {
        var obj=new GameObject(title,typeof(RectTransform),typeof(Image),typeof(Button)); obj.transform.SetParent(parent,false);
        var rect=(RectTransform)obj.transform; rect.sizeDelta=new Vector2(260,62); rect.anchoredPosition=position;
        obj.GetComponent<Image>().color=new Color(.72f,.88f,1f); var button=obj.GetComponent<Button>(); button.onClick.AddListener(click);
        var label=Label(obj.transform,title,Vector2.zero,22);label.rectTransform.sizeDelta=rect.sizeDelta; return button;
    }
}

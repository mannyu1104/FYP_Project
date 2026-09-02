using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the short capture flow used only by ForVideoScene.
/// </summary>
public class ForVideoSceneFlowController : MonoBehaviour
{
    private const string SceneName = "ForVideoScene";

    [Header("Auto Found References")]
    [SerializeField] private ComputerCanvasController computerCanvasController;
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private NPCDialogueTrigger clientNpc;

    [Header("Flow")]
    [SerializeField] private bool startClientDialogueAfterComputerCloses = true;
    [SerializeField] private string clientNpcNameKeyword = "\u59D4\u6258\u4EBA";
    [SerializeField] private List<string> alwaysVisibleObjectNames = new List<string>
    {
        "\u7535\u8111\uFF08\u8981\u94FE\u63A5mannyu\uFF09",
        "SettingButton(From InGame)"
    };

    private readonly List<GameObject> hiddenObjects = new List<GameObject>();
    private bool gameplayFlowStarted;
    private bool mainMenuWasVisible;
    private bool computerWasOpened;
    private bool tutorialScoreWasShown;
    private bool clientDialogueStarted;
    private bool clientDialogueFinished;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateForVideoSceneController()
    {
        if (SceneManager.GetActiveScene().name != SceneName)
        {
            return;
        }

        if (FindAnyObjectByType<ForVideoSceneFlowController>() != null)
        {
            return;
        }

        GameObject controllerObject = new GameObject("ForVideoSceneFlowController");
        controllerObject.AddComponent<ForVideoSceneFlowController>();
    }

    private void Awake()
    {
        ResolveReferences();
    }

    private void Start()
    {
        ResolveReferences();
        CountingPoint.ResetTutorialCompletion();
        tutorialScoreWasShown = false;
        HideClientNpc();
    }

    private void OnEnable()
    {
        CountingPoint.ScoreShown += OnTutorialScoreShown;
    }

    private void OnDisable()
    {
        CountingPoint.ScoreShown -= OnTutorialScoreShown;
    }

    private void LateUpdate()
    {
        ResolveReferences();

        if (IsMainMenuVisible())
        {
            mainMenuWasVisible = true;
            ResetFlowForNextNewGame();
            return;
        }

        if (!gameplayFlowStarted && mainMenuWasVisible && HasGameplayStarted())
        {
            StartGameplayCaptureFlow();
        }

        if (!gameplayFlowStarted || computerCanvasController == null)
        {
            return;
        }

        bool computerOpen = computerCanvasController.IsComputerOpen();
        if (computerOpen)
        {
            computerWasOpened = true;
            return;
        }

        if (computerWasOpened && tutorialScoreWasShown && startClientDialogueAfterComputerCloses && !clientDialogueStarted)
        {
            StartClientDialogue();
        }

        if (clientDialogueStarted && !clientDialogueFinished && dialogueController != null && !dialogueController.IsDialogueActive)
        {
            clientDialogueFinished = true;
            HideClientNpc();
        }
    }

    private void StartGameplayCaptureFlow()
    {
        gameplayFlowStarted = true;
        computerWasOpened = false;
        tutorialScoreWasShown = CountingPoint.HasScoreBeenShown;
        clientDialogueStarted = false;
        clientDialogueFinished = false;

        CountingPoint.ResetTutorialCompletion();
        tutorialScoreWasShown = false;
        HideAllInteractablesExceptVideoTargets();
        HideClientNpc();
    }

    private bool HasGameplayStarted()
    {
        GameObject gameRoot = FindSceneObjectByName("InGame");

        return gameRoot != null &&
            gameRoot.activeInHierarchy &&
            !IsMainMenuVisible();
    }

    private bool IsMainMenuVisible()
    {
        GameObject mainMenuPanel = FindSceneObjectByName("MainMenuPanel");
        return mainMenuPanel != null && mainMenuPanel.activeInHierarchy;
    }

    private void HideAllInteractablesExceptVideoTargets()
    {
        hiddenObjects.Clear();

        CursorInteractionTarget[] targets = FindObjectsByType<CursorInteractionTarget>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        for (int i = 0; i < targets.Length; i++)
        {
            CursorInteractionTarget target = targets[i];
            if (target == null || target.gameObject == null || ShouldKeepVisible(target.gameObject))
            {
                continue;
            }

            if (target.gameObject.activeSelf)
            {
                hiddenObjects.Add(target.gameObject);
                target.gameObject.SetActive(false);
            }
        }
    }

    private bool ShouldKeepVisible(GameObject target)
    {
        if (target.GetComponent<ComputerAccessPoint>() != null)
        {
            return true;
        }

        for (int i = 0; i < alwaysVisibleObjectNames.Count; i++)
        {
            string objectName = alwaysVisibleObjectNames[i];
            if (!string.IsNullOrWhiteSpace(objectName) && target.name == objectName)
            {
                return true;
            }
        }

        return false;
    }

    private void StartClientDialogue()
    {
        if (clientNpc == null)
        {
            return;
        }

        clientDialogueStarted = true;
        clientNpc.gameObject.SetActive(true);
        clientNpc.transform.SetAsLastSibling();
        clientNpc.ResetProgress();
        clientNpc.StartDialogue();
    }

    private void HideClientNpc()
    {
        if (clientNpc != null)
        {
            clientNpc.gameObject.SetActive(false);
        }
    }

    private void ResolveReferences()
    {
        if (computerCanvasController == null)
        {
            computerCanvasController = FindAnyObjectByType<ComputerCanvasController>();
        }

        if (dialogueController == null)
        {
            dialogueController = FindAnyObjectByType<DialogueController>();
        }

        if (clientNpc == null)
        {
            clientNpc = FindClientNpc();
        }
    }

    private NPCDialogueTrigger FindClientNpc()
    {
        NPCDialogueTrigger[] npcs = FindObjectsByType<NPCDialogueTrigger>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        for (int i = 0; i < npcs.Length; i++)
        {
            NPCDialogueTrigger npc = npcs[i];
            if (npc != null && npc.name.Contains(clientNpcNameKeyword))
            {
                return npc;
            }
        }

        return null;
    }

    private void OnTutorialScoreShown()
    {
        tutorialScoreWasShown = true;
    }

    private void ResetFlowForNextNewGame()
    {
        if (!gameplayFlowStarted && !computerWasOpened && !clientDialogueStarted)
        {
            return;
        }

        gameplayFlowStarted = false;
        computerWasOpened = false;
        tutorialScoreWasShown = false;
        clientDialogueStarted = false;
        clientDialogueFinished = false;
        CountingPoint.ResetTutorialCompletion();
        HideClientNpc();
    }

    private static GameObject FindSceneObjectByName(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return null;
        }

        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform target = transforms[i];
            if (target != null && target.name == objectName && target.gameObject.scene.IsValid())
            {
                return target.gameObject;
            }
        }

        return null;
    }
}

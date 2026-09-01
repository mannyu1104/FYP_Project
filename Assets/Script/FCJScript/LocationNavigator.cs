using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles navigation between Home and the orphanage areas.
/// </summary>
public class LocationNavigator : MonoBehaviour
{
    private const int EntranceBackButtonIndex = 0;
    private const int StaffRoomBackButtonIndex = 1;

    [Header("Look Controller")]
    [Tooltip("If empty, the navigator will find the first LookController in the scene.")]
    [SerializeField] private LookController lookController;

    [Header("Transition")]
    [SerializeField] private ScreenTransitionController screenTransitionController;
    [SerializeField] private bool useTransition = true;

    [Header("Location Containers")]
    [Tooltip("Container for the Home location.")]
    [SerializeField] private GameObject homeContainer;
    [Tooltip("Container for the orphanage location.")]
    [SerializeField] private GameObject orphanageContainer;

    [Header("Orphanage Area Images")]
    [Tooltip("Main orphanage image.")]
    [SerializeField] private GameObject mainImage;
    [Tooltip("Orphanage entrance image.")]
    [SerializeField] private GameObject entranceImage;
    [Tooltip("Orphanage staff room image.")]
    [SerializeField] private GameObject staffRoomImage;

    [Header("Orphanage Objects")]
    [Tooltip("Objects shown in the entrance area.")]
    [SerializeField] private GameObject entranceObjects;
    [Tooltip("Objects shown in the staff room.")]
    [SerializeField] private GameObject staffRoomObjects;
    [Tooltip("NPCs shown in the orphanage main or entrance area.")]
    [SerializeField] private GameObject npcContainer;

    [Header("LookController Scene Image Indices")]
    [Tooltip("Index of the Home image in LookController.sceneImages.")]
    [SerializeField] private int homeImageIndex = 0;
    [Tooltip("Index of the orphanage main image in LookController.sceneImages.")]
    [SerializeField] private int orphanageMainImageIndex = 1;

    [Header("Main Area Buttons")]
    [Tooltip("Buttons shown on the orphanage main area, such as the entrance button.")]
    [SerializeField] private GameObject[] mainAreaButtons = new GameObject[0];

    [Header("Entrance Area Buttons")]
    [Tooltip("Buttons shown only after entering the orphanage entrance area.")]
    [SerializeField] private GameObject[] entranceAreaButtons = new GameObject[0];

    [Header("Back Buttons")]
    [Tooltip("Back buttons. Element 0 is for entrance, element 1 is for staff room.")]
    [SerializeField] private GameObject[] backButtons = new GameObject[0];

    private void Reset()
    {
        TryFindSceneReferences();
        ResolveButtonGroups();
    }

    private void Awake()
    {
        TryFindSceneReferences();
        ResolveButtonGroups();
    }

    private void TryFindSceneReferences()
    {
        if (lookController == null)
        {
            lookController = FindAnyObjectByType<LookController>();
        }

        if (screenTransitionController == null)
        {
            screenTransitionController = FindAnyObjectByType<ScreenTransitionController>();
        }
    }

    private void ResolveButtonGroups()
    {
        if (!HasAnyAssigned(mainAreaButtons))
        {
            GameObject entranceButton = FindButtonByMethod(mainImage, nameof(GoToEntrance));
            mainAreaButtons = entranceButton != null
                ? new[] { entranceButton }
                : new GameObject[0];
        }

        if (!HasAnyAssigned(entranceAreaButtons))
        {
            GameObject staffRoomButton = FindButtonByMethod(entranceImage, nameof(GoToStaffRoom));
            entranceAreaButtons = staffRoomButton != null
                ? new[] { staffRoomButton }
                : new GameObject[0];
        }

        if (!HasAssignedAt(backButtons, EntranceBackButtonIndex) || !HasAssignedAt(backButtons, StaffRoomBackButtonIndex))
        {
            GameObject backToOrphanageButton = FindButtonByMethod(entranceImage, nameof(GoToOrphanage));
            GameObject backToEntranceButton = FindButtonByMethod(staffRoomImage, nameof(GoToEntrance));
            backButtons = new[] { backToOrphanageButton, backToEntranceButton };
        }
    }

    public void GoToOrphanage()
    {
        PlayWithTransition(GoToOrphanageImmediately);
    }

    private void GoToOrphanageImmediately()
    {
        SetGameObject(homeContainer, false);
        SetGameObject(orphanageContainer, true);

        ShowOrphanageMainArea();
    }

    public void BackToOrphanage()
    {
        PlayWithTransition(BackToOrphanageImmediately);
    }

    public void ShowOrphanageMainFromMap()
    {
        EnsureOrphanageShown();
        ShowOrphanageMainArea();
    }

    private void BackToOrphanageImmediately()
    {
        EnsureOrphanageShown();
        ShowOrphanageMainArea();
    }

    public void GoToEntrance()
    {
        PlayWithTransition(GoToEntranceImmediately);
    }

    private void GoToEntranceImmediately()
    {
        ResolveButtonGroups();
        EnsureOrphanageShown();

        SetGameObject(mainImage, false);
        SetGameObject(entranceImage, true);
        SetGameObject(staffRoomImage, false);

        SetGameObject(entranceObjects, true);
        SetGameObject(staffRoomObjects, false);
        SetGameObject(npcContainer, true);

        SetButtonGroup(mainAreaButtons, false);
        SetButtonGroup(entranceAreaButtons, true);
        SetAreaBackButton(EntranceBackButtonIndex);

        SetLookTo(entranceImage);
    }

    public void GoToStaffRoom()
    {
        PlayWithTransition(GoToStaffRoomImmediately);
    }

    private void GoToStaffRoomImmediately()
    {
        ResolveButtonGroups();
        EnsureOrphanageShown();

        SetGameObject(mainImage, false);
        SetGameObject(entranceImage, false);
        SetGameObject(staffRoomImage, true);

        SetGameObject(entranceObjects, false);
        SetGameObject(staffRoomObjects, true);
        SetGameObject(npcContainer, false);

        SetButtonGroup(mainAreaButtons, false);
        SetButtonGroup(entranceAreaButtons, false);
        SetAreaBackButton(StaffRoomBackButtonIndex);

        SetLookTo(staffRoomImage);
    }

    public void GoToHome()
    {
        PlayWithTransition(GoToHomeImmediately);
    }

    private void GoToHomeImmediately()
    {
        SetGameObject(homeContainer, true);
        SetGameObject(orphanageContainer, false);

        if (lookController != null)
        {
            lookController.SetSceneImage(homeImageIndex);
        }
    }

    private void PlayWithTransition(System.Action action)
    {
        TryFindSceneReferences();

        if (useTransition && screenTransitionController != null)
        {
            screenTransitionController.PlayTransition(action);
            return;
        }

        action?.Invoke();
    }

    private void EnsureOrphanageShown()
    {
        SetGameObject(homeContainer, false);
        SetGameObject(orphanageContainer, true);
    }

    private void ShowOrphanageMainArea()
    {
        ResolveButtonGroups();

        SetGameObject(mainImage, true);
        SetGameObject(entranceImage, false);
        SetGameObject(staffRoomImage, false);

        SetGameObject(entranceObjects, false);
        SetGameObject(staffRoomObjects, false);
        SetGameObject(npcContainer, true);

        SetButtonGroup(mainAreaButtons, true);
        SetButtonGroup(entranceAreaButtons, false);
        SetButtonGroup(backButtons, false);

        if (lookController != null)
        {
            lookController.SetSceneImage(orphanageMainImageIndex);
        }
    }

    private void SetLookTo(GameObject areaImage)
    {
        if (lookController == null || areaImage == null)
        {
            return;
        }

        RectTransform rectTransform = areaImage.transform as RectTransform;
        if (rectTransform != null)
        {
            lookController.ShowSceneImage(rectTransform);
        }
    }

    private void SetAreaBackButton(int visibleIndex)
    {
        if (backButtons == null)
        {
            return;
        }

        for (int i = 0; i < backButtons.Length; i++)
        {
            SetGameObject(backButtons[i], i == visibleIndex);
        }
    }

    private static void SetGameObject(GameObject go, bool active)
    {
        if (go == null)
        {
            return;
        }

        go.SetActive(active);
    }

    private static void SetButtonGroup(GameObject[] group, bool active)
    {
        if (group == null)
        {
            return;
        }

        for (int i = 0; i < group.Length; i++)
        {
            SetGameObject(group[i], active);
        }
    }

    private static bool HasAnyAssigned(GameObject[] group)
    {
        if (group == null)
        {
            return false;
        }

        for (int i = 0; i < group.Length; i++)
        {
            if (group[i] != null)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasAssignedAt(GameObject[] group, int index)
    {
        return group != null && index >= 0 && index < group.Length && group[index] != null;
    }

    private static GameObject FindButtonByMethod(GameObject parent, string methodName)
    {
        if (parent == null || string.IsNullOrWhiteSpace(methodName))
        {
            return null;
        }

        Button[] buttons = parent.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            int eventCount = button.onClick.GetPersistentEventCount();
            for (int eventIndex = 0; eventIndex < eventCount; eventIndex++)
            {
                if (button.onClick.GetPersistentMethodName(eventIndex) == methodName)
                {
                    return button.gameObject;
                }
            }
        }

        return null;
    }
}

using UnityEngine;

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
    [Tooltip("Buttons shown on the orphanage main area, such as entrance and staff room buttons.")]
    [SerializeField] private GameObject[] mainAreaButtons = new GameObject[0];

    [Header("Back Buttons")]
    [Tooltip("Back buttons. Element 0 is for entrance, element 1 is for staff room.")]
    [SerializeField] private GameObject[] backButtons = new GameObject[0];

    private void Reset()
    {
        TryFindLookController();
    }

    private void Awake()
    {
        TryFindLookController();
    }

    private void TryFindLookController()
    {
        if (lookController == null)
        {
            lookController = FindFirstObjectByType<LookController>();
        }

        if (screenTransitionController == null)
        {
            screenTransitionController = FindFirstObjectByType<ScreenTransitionController>();
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
        EnsureOrphanageShown();

        SetGameObject(mainImage, false);
        SetGameObject(entranceImage, true);
        SetGameObject(staffRoomImage, false);

        SetGameObject(entranceObjects, true);
        SetGameObject(staffRoomObjects, false);
        SetGameObject(npcContainer, true);

        SetButtonGroup(mainAreaButtons, false);
        SetAreaBackButton(EntranceBackButtonIndex);

        SetLookTo(entranceImage);
    }

    public void GoToStaffRoom()
    {
        PlayWithTransition(GoToStaffRoomImmediately);
    }

    private void GoToStaffRoomImmediately()
    {
        EnsureOrphanageShown();

        SetGameObject(mainImage, false);
        SetGameObject(entranceImage, false);
        SetGameObject(staffRoomImage, true);

        SetGameObject(entranceObjects, false);
        SetGameObject(staffRoomObjects, true);
        SetGameObject(npcContainer, false);

        SetButtonGroup(mainAreaButtons, false);
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
        TryFindLookController();

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
        SetGameObject(mainImage, true);
        SetGameObject(entranceImage, false);
        SetGameObject(staffRoomImage, false);

        SetGameObject(entranceObjects, false);
        SetGameObject(staffRoomObjects, false);
        SetGameObject(npcContainer, true);

        SetButtonGroup(mainAreaButtons, true);
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

        RectTransform rt = areaImage.transform as RectTransform;
        if (rt != null)
        {
            lookController.ShowSceneImage(rt);
        }
    }

    private void SetAreaBackButton(int visibleIndex)
    {
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
}

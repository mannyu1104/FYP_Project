using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MapIcon : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Outline hoverOutline;
    private bool pointerInside;
    public void OnPointerEnter(PointerEventData eventData) { pointerInside = true; RefreshHover(); }
    public void OnPointerExit(PointerEventData eventData) { pointerInside = false; RefreshHover(); }
    private void OnDisable() { pointerInside = false; RefreshHover(); }
    private void Update() => RefreshHover();
    private void RefreshHover()
    {
        if (hoverOutline == null) return;
        var menu = FindAnyObjectByType<MainMenuController>();
        hoverOutline.enabled = pointerInside && thisUnlocked &&
            (thisID == 0 || thisID == 1 || thisID == 3 || thisID == 4) &&
            !SaveSlotPanel.IsOpen && (menu == null || !menu.IsSettingsVisible);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        EnsureInitialized();
        if (!thisUnlocked) return;
        var menu = FindAnyObjectByType<MainMenuController>();
        if (SaveSlotPanel.IsOpen || (menu != null && menu.IsSettingsVisible)) return;
        var navigator = FindAnyObjectByType<MapPanelNavigator>();
        if (navigator == null) return;
        // Map asset IDs differ from the old location-panel indices.
        switch (thisID)
        {
            case 0: navigator.OpenHome(); break;
            case 1: navigator.OpenPark(); break;
            case 3: navigator.OpenOrphanage(); break;
            case 4: navigator.OpenWelfare(); break;
        }
    }
    public Map Mapdetials;
    [SerializeField] private TMP_Text SumShowText;
    [SerializeField] private Sprite LockedFrame;
    [SerializeField] private Sprite UnlockedFrame;
    public Image image;
    public Image imageName;
    public int thisID;
    public bool thisUnlocked;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool initialized;

    void Start() => EnsureInitialized();

    public void EnsureInitialized()
    {
        if (initialized) return;
        initialized = true;
        InitialiseItem(Mapdetials);

        if (thisUnlocked)
        {
            image.sprite = Mapdetials.ImageUnlocked;
            imageName.sprite = UnlockedFrame;
            //SumShowText.text = Mapdetials.PlaceName;
            SumShowText.text = Mapdetials.PlaceNameLocalized.GetLocalizedString();

        }
        if (!thisUnlocked)
        {
            NotUnlock();
        }
    }

    public void InitialiseItem(Map newmap)
    {
        Mapdetials = newmap;
        thisID = newmap.MapID;
        thisUnlocked = newmap.Unlocked;
    }

    public void Unlocking()
    {
        thisUnlocked = true;
        image.sprite = Mapdetials.ImageUnlocked;
        imageName.sprite = UnlockedFrame;
        //SumShowText.text = Mapdetials.PlaceName;
        SumShowText.text = Mapdetials.PlaceNameLocalized.GetLocalizedString();

    }

    public void NotUnlock()
    {
        thisUnlocked = false;
        image.sprite = Mapdetials.ImageLocked;
        imageName.sprite = LockedFrame;
        SumShowText.text = "Unknown";
    }
}

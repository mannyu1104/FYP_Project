using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class MapIcon : MonoBehaviour
{
    public Map Mapdetials;
    [SerializeField] private TMP_Text SumShowText;
    [SerializeField] private Sprite LockedFrame;
    [SerializeField] private Sprite UnlockedFrame;
    public Image image;
    public Image imageName;
    public bool thisUnlocked;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitialiseItem(Mapdetials);

        if (thisUnlocked)
        {
            image.sprite = Mapdetials.ImageUnlocked;
            imageName.sprite = UnlockedFrame;
            SumShowText.text = Mapdetials.PlaceName;
        }
        if (!thisUnlocked)
        {
            image.sprite = Mapdetials.ImageLocked;
            imageName.sprite = LockedFrame;
        }
    }

    public void InitialiseItem(Map newmap)
    {
        Mapdetials = newmap;
        thisUnlocked = newmap.Unlocked;
    }

    public void Unlocking()
    {
        thisUnlocked = true;
        image.sprite = Mapdetials.ImageUnlocked;
        imageName.sprite = UnlockedFrame;
    }
}

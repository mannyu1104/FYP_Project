using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObject/Item")]
public class Item : ScriptableObject
{
    public Sprite Image;
    public bool Unlocked;
    public bool Get;
    public bool Used;
    public int ItemID;
}

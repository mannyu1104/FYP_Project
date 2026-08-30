using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObject/Item")]
public class Item : ScriptableObject
{
    public string ItemShowName;
    public string Description;
    public bool Get;
    public bool Used;
    public bool Show;
    public int ItemID;
}

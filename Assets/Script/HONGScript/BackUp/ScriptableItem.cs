using UnityEngine;

[CreateAssetMenu(fileName = "MapItem", menuName = "ScriptableObject/MapItem")]
public class MapItem : ScriptableObject
{
    public Sprite Image;
    public bool Get;
    public bool Used;
    public bool Show;
    public int ItemID;
}

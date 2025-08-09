using UnityEngine;

[CreateAssetMenu(fileName = "PlaceableItem", menuName = "Placeable Items/Placeable Item")]
public class PlaceableItem : ScriptableObject
{
    public enum ItemType { Object, Terrain, Unit }
    public enum TerrainType
    {
        None, Grass, Sand, Water, Rock, Gravel, DirtRoad, Hill, Forest, Asphalt, Mud, Snow
    }

    public string itemName;
    public Sprite itemImage;
    public GameObject itemPrefab;

    public ItemType itemType;
    public TerrainType terrainType;

    public int unitHealth;
    public string unitFaction;
}
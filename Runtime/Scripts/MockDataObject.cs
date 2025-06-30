using UnityEngine;

[CreateAssetMenu(fileName = "New MockData", menuName = "Importer Example/Mock Data Object")]
public class MockDataObject : ScriptableObject
{
    public string itemName;
    [TextArea] public string description;
    public ItemType itemType;
    public bool isUnique;
    public int value;
    public float weight;

    public enum ItemType
    {
        None,
        Weapon,
        Armor,
        Potion,
        QuestItem
    }
}
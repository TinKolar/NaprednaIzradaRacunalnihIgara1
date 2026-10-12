using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
        public string ItemName = "Item";
    public string Slot = "head"; // head, armour, gloves, boots, ring, amulet
    public Sprite Icon;
    public string Rarity = "common"; // common, magic, rare
    [TextArea] public string Description;

    [Header("Bonusi")]

    public int Damage;
}

public enum Rarity
{
    Common =1,
    Rare=2,
    Epic=3,
    Legendary=4
}

public enum ItemType
{
    Head=0,
    Weapon=1,
    Shield=2,
    Ring=3,
    Necklace=4,
    Chest=5,
    Boots=6,
    Gloves=7,
    Bag=8
}
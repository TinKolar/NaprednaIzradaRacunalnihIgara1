using UnityEngine;
using UnityEngine.Serialization;

//Bazni podaci za svaki item: ime, ikona, rarity, opis
//Nasljeduju ga EquippableData (oprema i oruzje) i ConsumableData (potioni)
public abstract class ItemData : ScriptableObject
{
    [FormerlySerializedAs("WeaponName")]
    public string ItemName = "Item";
    public Sprite Icon;
    public Rarity Rarity = Rarity.Common;
    [TextArea] public string Description;
    [Tooltip("1 = ne stacka se")]
    public int MaxStack = 1;
}

//Enumi se spremaju kao broj! Nove vrijednosti dodavati samo na kraj i ne mijenjati postojece brojeve.
public enum Rarity
{
    Common = 0,
    Rare = 1,
    Epic = 2,
    Legendary = 3
}

public enum EquipType
{
    Head = 0,
    Chest = 1,
    Gloves = 2,
    Boots = 3,
    Ring = 4,
    Necklace = 5,
    OneHanded = 6,
    TwoHanded = 7,
    Shield = 8
}

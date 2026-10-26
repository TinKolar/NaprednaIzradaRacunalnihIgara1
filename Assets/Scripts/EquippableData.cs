using UnityEngine;

//Sve sto se moze equipat (oprema i oruzje). Bonusi idu u PlayerStats kao modifieri (PlayerStats.ApplyEquipment).
public abstract class EquippableData : ItemData
{
    public EquipType EquipType;

    [Header("Bonus")]
    public int Damage;
    public int Armour;
    public int MaxHealth;
    public int Speed;
}

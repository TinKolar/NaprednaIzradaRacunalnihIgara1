using UnityEngine;

//Sve sto se moze equipat (oprema i oruzje). Bonusi se zbrajaju u Player_Controler.RecalculateStats.
public abstract class EquippableData : ItemData
{
    public EquipType EquipType;

    [Header("Bonus")]
    public int Damage;
    public int Armour;
    public int MaxHealth;
    public int Speed;
}

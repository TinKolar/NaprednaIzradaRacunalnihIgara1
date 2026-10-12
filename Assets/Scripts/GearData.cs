using UnityEngine;

//Oprema koja se moze equipat (kaciga, oklop, rukavice, cizme, prsten, ogrlica)
[CreateAssetMenu(fileName = "NewGear", menuName = "Game/Gear Data")]
public class GearData : ItemData
{
    [Header("Bonusi")]
    public int Armour;
    public int MaxHealth;

    public int Speed;
}

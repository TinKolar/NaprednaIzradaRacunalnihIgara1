using UnityEngine;

//Itemi koji se potrose (potion). Desni klik u inventoryju = popij.
[CreateAssetMenu(fileName = "NewConsumable", menuName = "Game/Consumable Data")]
public class ConsumableData : ItemData
{
    public int HealAmount = 30;
}

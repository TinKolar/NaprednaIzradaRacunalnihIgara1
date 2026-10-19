using UnityEngine;

//Slot na liku (equipment panel). Item opisuje sebe (EquipType), slot opisuje pravila (sto prima).
public class EquipSlot : MonoBehaviour
{
    public EquipType[] Accepts;

    public bool CanEquip(EquippableData item)
    {
        return item != null && System.Array.IndexOf(Accepts, item.EquipType) >= 0;
    }

    //item koji je trenutno u slotu (ili null)
    public Inventory_LootItem Current
    {
        get { return transform.childCount > 0 ? transform.GetChild(0).GetComponent<Inventory_LootItem>() : null; }
    }
}

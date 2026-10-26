using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class Player_Inventory : MonoBehaviour
{
    [SerializeField]
    public static List<GameObject> Inventory = new List<GameObject>();
    public static GameObject EmptySlot;
    public static string SlotPointer = "";
    public static string DragedItemType = "";
    //Slotovi na igracu
    public GameObject GloveSlot;
    public GameObject HeadSlot;
    public GameObject RingSlot;
    public GameObject WeaponSlot;
    public GameObject ArmourSlot;
    public GameObject ShieldSlot;
    public GameObject BootsSlot;
    public GameObject BagSlot;
    public GameObject AmuletSlot;
    public GameObject Ring2Slot;

    //svi slotovi na liku koji imaju EquipSlot (main hand = WeaponSlot, off hand = ShieldSlot)
    public List<EquipSlot> EquipSlots()
    {
        List<EquipSlot> list = new List<EquipSlot>();
        GameObject[] all = { HeadSlot, ArmourSlot, GloveSlot, BootsSlot, AmuletSlot, RingSlot, Ring2Slot, WeaponSlot, ShieldSlot };
        foreach (GameObject go in all)
        {
            if (go != null && go.GetComponent<EquipSlot>() != null) list.Add(go.GetComponent<EquipSlot>());
        }
        return list;
    }

    public GameObject InventoryGrid;
    public GameObject InventorySlot;

    private GameObject _player;
    public UnityEvent OnChangedBag;
    // Start is called before the first frame update
    void Start()
    {       
        InventorySlotsHandler();
        _player = GameObject.Find("Player");
        OnChangedBag.AddListener(InventorySlotsHandler);
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            Player_Controler.InventoryCapacity += 8;
            InventorySlotsHandler();
            Debug.Log(Player_Controler.InventoryCapacity);
        }
    }
    // Postavlja kolicinu slotova u listu Inventory i InventoryGrid s obzirom na maksimalni kapacitet
    private void InventorySlotsHandler()
    {
        int currentSlotsNum = Inventory.Count;
        if(currentSlotsNum < Player_Controler.InventoryCapacity)
        {           
            for (int i = 0; i < Player_Controler.InventoryCapacity - currentSlotsNum; i++)
            {
                GameObject slot = GameObject.Instantiate(InventorySlot, InventoryGrid.transform);
                Inventory.Add(slot);
            }
            InventoryGrid.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Player_Controler.InventoryCapacity / 8 * 41);
        }
        else if(currentSlotsNum > Player_Controler.InventoryCapacity)
        {            
            for (int i = Player_Controler.InventoryCapacity; i < currentSlotsNum; i++)
            {
                bool hasEmptySlot = false;
                int newCapacityCounter = Player_Controler.InventoryCapacity;
                if (Inventory[i].transform.childCount > 0)
                {   
                    //Uslucaju da ima praznih mjesta u inventoriju, premjesta iteme na prazna mjesta, a ako nema, dropa iteme van
                    foreach (GameObject slot in Inventory)
                    {                      
                        if(newCapacityCounter == 0)
                        {
                            break;
                        }
                        if (slot.transform.childCount == 0)
                        {
                            hasEmptySlot = true;
                            Inventory[i].transform.GetChild(0).transform.SetParent(slot.transform);
                            slot.transform.GetChild(0).localPosition = Vector2.zero;
                            break;
                        }
                        newCapacityCounter--;
                    }
                    if (!hasEmptySlot)
                    {
                        DropItemHandler(Inventory[i]);
                    }
                }
                GameObject.Destroy(Inventory[i]);
            }
           
            Inventory.RemoveRange(Player_Controler.InventoryCapacity, currentSlotsNum - Player_Controler.InventoryCapacity);
            InventoryGrid.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Player_Controler.InventoryCapacity / 8 * 41);
        }
    }
    // Ispusta item iz inventorija na pod
    private void DropItemHandler(GameObject item)
    {
        //TODO: bacit item na pod kao loot vrecu (stari Drag_and_drop.DropItem je obrisan)
        RectTransform.Destroy(item.gameObject);
        Debug.Log("No free slots in inventory! Items are destroyed!");
    }
}

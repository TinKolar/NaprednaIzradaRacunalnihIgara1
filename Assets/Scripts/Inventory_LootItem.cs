using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

//Item u inventoryju koji dolazi iz loota. Desni klik: oruzje = equip/unequip, potion = popij.
public class Inventory_LootItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public string ItemName;
    public ItemType itemType; // weapon, potion, gear
    public WeaponData Weapon;
    public GearData Gear;

    public void OnPointerEnter(PointerEventData eventData)
    {
        ItemTooltip.Show(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltip.Hide();
    }

    void OnDestroy()
    {
        ItemTooltip.Hide();
    }

    //u koji equipment slot ide oprema
    public static GameObject SlotFor(Player_Inventory inv, string slot)
    {
        if (slot == "head") return inv.HeadSlot;
        if (slot == "armour") return inv.ArmourSlot;
        if (slot == "gloves") return inv.GloveSlot;
        if (slot == "boots") return inv.BootsSlot;
        if (slot == "ring") return inv.RingSlot;
        if (slot == "amulet") return inv.AmuletSlot;
        return null;
    }

    public bool IsEquipped()
    {
        Player_Inventory inv = GameObject.Find("GameManager").GetComponent<Player_Inventory>();
        if (itemType == ItemType.Weapon) return transform.parent == inv.WeaponSlot.transform;
        if (itemType == ItemType.Necklace && Gear != null)
        {
            GameObject slot = SlotFor(inv, Gear.Slot);
            return slot != null && transform.parent == slot.transform;
        }
        return false;
    }
    public int Amount = 1;
    public int HealAmount = 30;
    public TextMeshProUGUI AmountText;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Use();
        }
    }

    public void Use()
    {
        Player_Inventory inv = GameObject.Find("GameManager").GetComponent<Player_Inventory>();
        Player_Controler player = GameObject.Find("Player").GetComponent<Player_Controler>();

        if (itemType == ItemType.Weapon)
        {
            if (transform.parent == inv.WeaponSlot.transform)
            {
                //skini oruzje u inventory
                Transform empty = FindEmptySlot();
                if (empty == null)
                {
                    Debug.Log("Inventory is full!");
                    return;
                }
                transform.SetParent(empty, false);
                Fit(transform);
                player.EquipWeapon(null);
            }
            else
            {
                //zamijeni s oruzjem koje je trenutno u ruci
                Transform from = transform.parent;
                if (inv.WeaponSlot.transform.childCount > 0)
                {
                    Transform old = inv.WeaponSlot.transform.GetChild(0);
                    old.SetParent(from, false);
                    Fit(old);
                }
                transform.SetParent(inv.WeaponSlot.transform, false);
                Fit(transform);
                player.EquipWeapon(Weapon);
            }
            SFX.Play(SFX.Instance.Click, 0.6f);
        }
        else if (temType == "gear" && Gear != null)
        {
            GameObject gearSlot = SlotFor(inv, Gear.Slot);
            if (gearSlot == null) return;
            if (transform.parent == gearSlot.transform)
            {
                //skini u inventory
                Transform empty = FindEmptySlot();
                if (empty == null)
                {
                    Debug.Log("Inventory is full!");
                    return;
                }
                transform.SetParent(empty, false);
                Fit(transform);
            }
            else
            {
                //zamijeni s onim sto je trenutno u slotu
                Transform from = transform.parent;
                if (gearSlot.transform.childCount > 0)
                {
                    Transform old = gearSlot.transform.GetChild(0);
                    old.SetParent(from, false);
                    Fit(old);
                }
                transform.SetParent(gearSlot.transform, false);
                Fit(transform);
            }
            Player_Controler.RecalculateStats();
            SFX.Play(SFX.Instance.Click, 0.6f);
            ItemTooltip.Show(this);
        }
        else if (ItemType == "potion")
        {
            if (Player_Controler.CurrentHealth >= Player_Controler.MaxHealth || player.isDead)
            {
                return;
            }
            Player_Controler.CurrentHealth = Mathf.Min(Player_Controler.MaxHealth, Player_Controler.CurrentHealth + HealAmount);
            UI_Controler ui = GameObject.Find("GameManager").GetComponent<UI_Controler>();
            ui.OnChangedHealth.Invoke();
            SFX.Play(SFX.Instance.DrinkPotion);
            Amount = Amount - 1;
            RefreshAmount();
            if (Amount <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    public void RefreshAmount()
    {
        if (AmountText != null)
        {
            AmountText.text = Amount > 1 ? Amount.ToString() : "";
        }
    }

    public static Transform FindEmptySlot()
    {
        foreach (GameObject slot in Player_Inventory.Inventory)
        {
            if (slot.transform.childCount == 0)
            {
                return slot.transform;
            }
        }
        return null;
    }

    public static void Fit(Transform t)
    {
        RectTransform rt = (RectTransform)t;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(3, 3);
        rt.offsetMax = new Vector2(-3, -3);
        rt.localScale = Vector3.one;
    }

    //Napravi item u slotu iz LootItem podataka
    public static Inventory_LootItem Create(LootItem item, Transform slot)
    {
        GameObject go = new GameObject(item.Name, typeof(RectTransform));
        go.transform.SetParent(slot, false);
        Fit(go.transform);

        Image img = go.AddComponent<Image>();
        Sprite icon = item.Icon;
        if (icon == null && item.Weapon != null) icon = item.Weapon.Icon;
        if (icon == null && item.Gear != null) icon = item.Gear.Icon;
        if (icon != null)
        {
            img.sprite = icon;
            img.preserveAspect = true;
        }
        else
        {
            img.color = new Color(0.3f, 0.6f, 0.3f);
        }

        GameObject txt = new GameObject("Amount", typeof(RectTransform));
        txt.transform.SetParent(go.transform, false);
        RectTransform trt = txt.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.4f, 0);
        trt.anchorMax = new Vector2(1, 0.45f);
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        TextMeshProUGUI t = txt.AddComponent<TextMeshProUGUI>();
        t.font = TMP_Settings.defaultFontAsset;
        t.fontSize = 15;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.BottomRight;
        t.color = new Color(0.2f, 0.11f, 0.05f);
        t.raycastTarget = false;

        Inventory_LootItem li = go.AddComponent<Inventory_LootItem>();
        li.ItemName = item.Name;
        li.ItemType = item.ItemType;
        li.Weapon = item.Weapon;
        li.Gear = item.Gear;
        li.Amount = item.Amount;
        li.HealAmount = item.HealAmount;
        li.AmountText = t;
        li.RefreshAmount();
        return li;
    }
}

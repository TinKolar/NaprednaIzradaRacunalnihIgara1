using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

//Item u inventoryju koji dolazi iz loota. Desni klik: oprema/oruzje = equip/unequip, potion = popij.
public class Inventory_LootItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public ItemData Data;
    public int Amount = 1;
    public TextMeshProUGUI AmountText;

    public string ItemName { get { return Data != null ? Data.ItemName : name; } }

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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Use();
        }
    }

    //slot na liku u kojem je item (null ako je u inventoryju)
    public EquipSlot EquippedIn()
    {
        return transform.parent != null ? transform.parent.GetComponent<EquipSlot>() : null;
    }

    public bool IsEquipped()
    {
        return EquippedIn() != null;
    }

    public void Use()
    {
        if (Data is ConsumableData)
        {
            Drink((ConsumableData)Data);
        }
        else if (Data is EquippableData)
        {
            if (IsEquipped()) Unequip();
            else Equip((EquippableData)Data);
        }
    }

    void Drink(ConsumableData potion)
    {
        Player_Controler player = GameObject.Find("Player").GetComponent<Player_Controler>();
        Resource health = player.Stats.Health;
        if (health.Current >= health.Max || player.isDead)
        {
            return;
        }
        health.Increase(potion.HealAmount);
        SFX.Play(SFX.Instance.DrinkPotion);
        Amount = Amount - 1;
        RefreshAmount();
        if (Amount <= 0)
        {
            Destroy(gameObject);
        }
    }

    void Unequip()
    {
        Transform empty = FindEmptySlot();
        if (empty == null)
        {
            Debug.Log("Inventory is full!");
            return;
        }
        Move(transform, empty);
        AfterEquipChange();
    }

    void Equip(EquippableData item)
    {
        Player_Inventory inv = GameObject.Find("GameManager").GetComponent<Player_Inventory>();
        EquipSlot target = FindTargetSlot(inv, item);
        if (target == null)
        {
            Debug.Log("Nema slota za " + item.ItemName);
            return;
        }

        //sve sto mora van iz slotova: ono sto je trenutno u ciljnom slotu + pravila za dvoručno oruzje
        List<Inventory_LootItem> displaced = new List<Inventory_LootItem>();
        if (target.Current != null) displaced.Add(target.Current);
        EquipSlot mainHand = inv.WeaponSlot.GetComponent<EquipSlot>();
        EquipSlot offHand = inv.ShieldSlot.GetComponent<EquipSlot>();
        if (item.EquipType == EquipType.TwoHanded && target == mainHand && offHand.Current != null)
        {
            displaced.Add(offHand.Current); //dvoručno oruzje oslobodi off hand
        }
        if (target == offHand && IsTwoHandedIn(mainHand))
        {
            displaced.Add(mainHand.Current); //stit/oruzje u off hand skida dvoručno oruzje
        }

        //prvi izbaceni ide na mjesto ovog itema, ostali traze prazan slot u inventoryju
        Transform from = transform.parent;
        List<Transform> free = new List<Transform>();
        free.Add(from);
        for (int i = 1; i < displaced.Count; i++)
        {
            Transform empty = FindEmptySlot(free);
            if (empty == null)
            {
                Debug.Log("Inventory is full!");
                return;
            }
            free.Add(empty);
        }

        Move(transform, target.transform);
        for (int i = 0; i < displaced.Count; i++)
        {
            Move(displaced[i].transform, free[i]);
        }
        AfterEquipChange();
    }

    //prvi slobodan slot koji prima item, a ako ga nema prvi koji ga prima (zamjena)
    static EquipSlot FindTargetSlot(Player_Inventory inv, EquippableData item)
    {
        EquipSlot mainHand = inv.WeaponSlot.GetComponent<EquipSlot>();
        //dvoručno oruzje uvijek ide u main hand
        if (item.EquipType == EquipType.TwoHanded) return mainHand.CanEquip(item) ? mainHand : null;

        EquipSlot firstMatch = null;
        foreach (EquipSlot slot in inv.EquipSlots())
        {
            if (!slot.CanEquip(item)) continue;
            //off hand je "zauzet" dok je u main handu dvoručno oruzje
            bool blocked = slot.gameObject == inv.ShieldSlot && IsTwoHandedIn(mainHand) && item.EquipType != EquipType.Shield;
            if (slot.Current == null && !blocked) return slot;
            if (firstMatch == null) firstMatch = slot;
        }
        return firstMatch;
    }

    static bool IsTwoHandedIn(EquipSlot slot)
    {
        Inventory_LootItem current = slot.Current;
        return current != null && current.Data is EquippableData && ((EquippableData)current.Data).EquipType == EquipType.TwoHanded;
    }

    void AfterEquipChange()
    {
        Player_Controler player = GameObject.Find("Player").GetComponent<Player_Controler>();
        player.RefreshEquipment();
        SFX.Play(SFX.Instance.Click, 0.6f);
        ItemTooltip.Show(this);
    }

    static void Move(Transform item, Transform slot)
    {
        item.SetParent(slot, false);
        Fit(item);
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
        return FindEmptySlot(null);
    }

    //prazan slot u inventoryju koji nije u listi "taken"
    public static Transform FindEmptySlot(List<Transform> taken)
    {
        foreach (GameObject slot in Player_Inventory.Inventory)
        {
            if (slot.transform.childCount == 0 && (taken == null || !taken.Contains(slot.transform)))
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

    //Napravi item u slotu iz ItemData
    public static Inventory_LootItem Create(ItemData data, int amount, Transform slot)
    {
        GameObject go = new GameObject(data.ItemName, typeof(RectTransform));
        go.transform.SetParent(slot, false);
        Fit(go.transform);

        Image img = go.AddComponent<Image>();
        if (data.Icon != null)
        {
            img.sprite = data.Icon;
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
        li.Data = data;
        li.Amount = amount;
        li.AmountText = t;
        li.RefreshAmount();
        return li;
    }
}

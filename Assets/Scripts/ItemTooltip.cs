using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

//Tooltip za iteme iz loota (oprema, oruzje, potion): ime, vrsta, statsi i uputa
public class ItemTooltip : MonoBehaviour
{
    public static ItemTooltip Instance;

    public RectTransform Panel;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI TypeText;
    public TextMeshProUGUI StatsText;
    public TextMeshProUGUI HintText;
    public Vector2 Offset = new Vector2(18, -18);

    void Awake()
    {
        Instance = this;
        Panel.gameObject.SetActive(false);
    }

    public static void Show(Inventory_LootItem item)
    {
        if (Instance == null || item == null) return;
        Instance.Fill(item);
        Instance.Panel.gameObject.SetActive(true);
        Instance.FollowMouse();
    }

    public static void Hide()
    {
        if (Instance == null) return;
        Instance.Panel.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (Panel.gameObject.activeSelf) FollowMouse();
    }

    void FollowMouse()
    {
        if (Mouse.current == null) return;
        Vector2 mouse = Mouse.current.position.ReadValue();
        Canvas canvas = Panel.GetComponentInParent<Canvas>();
        float scale = canvas != null ? canvas.scaleFactor : 1;
        //ako bi izasao van ekrana okreni ga na drugu stranu misa
        Vector2 size = Panel.rect.size * scale;
        bool flipX = mouse.x + Offset.x * scale + size.x > Screen.width;
        bool flipY = mouse.y + Offset.y * scale - size.y < 0;
        Panel.pivot = new Vector2(flipX ? 1 : 0, flipY ? 0 : 1);
        Vector2 off = new Vector2(flipX ? -Offset.x : Offset.x, flipY ? -Offset.y : Offset.y) * scale;
        Panel.position = mouse + off;
    }

    void Fill(Inventory_LootItem item)
    {
        string stats = "";
        string type = "";
        string hint = "";
        string green = "<color=#2E6B1A>";
        ItemData data = item.Data;
        Color nameColor = RarityColor(data != null ? data.Rarity : Rarity.Common);

        if (data is EquippableData)
        {
            EquippableData e = (EquippableData)data;
            type = TypeName(e.EquipType);
            if (e.Damage != 0) stats += green + "+" + e.Damage + " Damage</color>\n";
            if (e.Armour != 0) stats += green + "+" + e.Armour + " Armour</color>\n";
            if (e.MaxHealth != 0) stats += green + "+" + e.MaxHealth + " Max HP</color>\n";
            if (e.Speed != 0) stats += green + "+" + e.Speed + " Speed</color>\n";
            //oruzje ima jos i podatke o napadu
            if (data is WeaponData)
            {
                WeaponData w = (WeaponData)data;
                stats += "Attack speed " + w.AttackSpeed.ToString("0.0") + "x\n";
                stats += "Reach " + w.LightRange.ToString("0.0") + " m\n";
                stats += "Whirlwind (Q): x" + w.HeavyDamageMultiplier + " damage, " + w.HeavyRadius.ToString("0.0") + " m\n";
            }
            hint = item.IsEquipped() ? "Right click: Unequip" : "Right click: Equip";
        }
        else if (data is ConsumableData)
        {
            type = "Potion";
            stats += green + "Restores " + ((ConsumableData)data).HealAmount + " HP</color>\n";
            stats += "Amount: " + item.Amount + "\n";
            hint = "Right click: Drink";
        }
        if (data != null && !string.IsNullOrEmpty(data.Description)) stats += "<i>" + data.Description + "</i>\n";

        NameText.text = item.ItemName + (item.IsEquipped() ? " <size=70%>(equipped)</size>" : "");
        NameText.color = nameColor;
        TypeText.text = type;
        StatsText.text = stats.TrimEnd('\n');
        HintText.text = hint;
    }

    static Color RarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Rare: return new Color(0.15f, 0.3f, 0.75f);
            case Rarity.Epic: return new Color(0.5f, 0.15f, 0.65f);
            case Rarity.Legendary: return new Color(0.78f, 0.45f, 0.02f);
        }
        return new Color(0.25f, 0.15f, 0.08f);
    }

    static string TypeName(EquipType type)
    {
        switch (type)
        {
            case EquipType.Head: return "Helmet";
            case EquipType.Chest: return "Body Armour";
            case EquipType.Gloves: return "Gloves";
            case EquipType.Boots: return "Boots";
            case EquipType.Ring: return "Ring";
            case EquipType.Necklace: return "Amulet";
            case EquipType.OneHanded: return "One-Handed Weapon";
            case EquipType.TwoHanded: return "Two-Handed Weapon";
            case EquipType.Shield: return "Shield";
        }
        return type.ToString();
    }
}

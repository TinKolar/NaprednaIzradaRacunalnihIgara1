using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//PLACEHOLDER prozor za loot dok se ne napravi pravi
public class Loot_Window : MonoBehaviour
{
    public GameObject Window;
    public Transform ItemList;
    public GameObject ItemButtonTemplate;
    public Button TakeAll;
    public Button Close;
    public UI_Controler UiControler;

    public Loot_Bag currentBag;
    private List<GameObject> buttons = new List<GameObject>();

    void Start()
    {
        TakeAll.onClick.AddListener(takeAll);
        Close.onClick.AddListener(CloseWindow);
        ItemButtonTemplate.SetActive(false);
        Window.SetActive(false);
    }

    void Update()
    {
        //zatvori ako se player udalji
        if (currentBag != null && Window.activeSelf)
        {
            GameObject player = GameObject.Find("Player");
            if (Vector3.Distance(player.transform.position, currentBag.transform.position) > 3)
            {
                CloseWindow();
            }
        }
    }

    public void Open(Loot_Bag bag)
    {
        currentBag = bag;
        if (Window.activeSelf == false) SFX.Play(SFX.Instance.OpenWindow);
        Window.SetActive(true);
        Refresh();
    }

    public void CloseWindow()
    {
        Window.SetActive(false);
        currentBag = null;
    }

    public void Refresh()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            Destroy(buttons[i]);
        }
        buttons.Clear();

        if (currentBag == null || currentBag.Items.Count == 0)
        {
            CloseWindow();
            return;
        }

        for (int i = 0; i < currentBag.Items.Count; i++)
        {
            LootItem item = currentBag.Items[i];
            GameObject b = Instantiate(ItemButtonTemplate, ItemList);
            b.SetActive(true);
            if (item.Amount > 1)
                b.GetComponentInChildren<TextMeshProUGUI>().text = item.Name + " x" + item.Amount;
            else
                b.GetComponentInChildren<TextMeshProUGUI>().text = item.Name;
            b.GetComponent<Button>().onClick.AddListener(() => takeItem(item));
            buttons.Add(b);
        }
    }

    private void takeItem(LootItem item)
    {
        if (item.Ruples)
        {
            Player_Controler.RupleCurrency += item.Amount;
            UiControler.OnChangedRuples.Invoke();
            SFX.Play(SFX.Instance.Coins);
            currentBag.Items.Remove(item);
            Refresh();
            return;
        }

        //itemi s MaxStack > 1 (potioni) se stackaju na postojeci
        if (item.Item != null && item.Item.MaxStack > 1)
        {
            foreach (GameObject slot in Player_Inventory.Inventory)
            {
                Inventory_LootItem existing = slot.GetComponentInChildren<Inventory_LootItem>();
                if (existing != null && existing.Data == item.Item && existing.Amount + item.Amount <= item.Item.MaxStack)
                {
                    existing.Amount += item.Amount;
                    existing.RefreshAmount();
                    Debug.Log("Looted " + item.Name + " x" + item.Amount);
                    currentBag.Items.Remove(item);
                    Refresh();
                    return;
                }
            }
        }

        //trazi prazan slot (isto ko u Collect_Items)
        foreach (GameObject slot in Player_Inventory.Inventory)
        {
            if (slot.transform.childCount == 0)
            {
                GameObject newItem;
                if (item.InventoryItem != null)
                {
                    newItem = Instantiate(item.InventoryItem, slot.transform);
                    newItem.transform.localPosition = Vector2.zero;
                    if (newItem.GetComponent<Stack>() != null)
                    {
                        newItem.GetComponent<Stack>().StackAmout = item.Amount;
                        newItem.GetComponent<Stack>().OnValueChanged.Invoke();
                    }
                }
                else if (item.Item != null)
                {
                    newItem = Inventory_LootItem.Create(item.Item, item.Amount, slot.transform).gameObject;
                }
                else
                {
                    newItem = makePlaceholder(item, slot.transform);
                }
                Debug.Log("Looted " + item.Name);
                currentBag.Items.Remove(item);
                Refresh();
                return;
            }
        }
        Debug.Log("Inventory is full!");
    }

    private void takeAll()
    {
        while (currentBag != null && currentBag.Items.Count > 0)
        {
            int before = currentBag.Items.Count;
            takeItem(currentBag.Items[0]);
            if (currentBag != null && currentBag.Items.Count == before)
            {
                break; //inventory pun
            }
        }
    }

    //privremena ikona dok nema pravih item prefaba
    private GameObject makePlaceholder(LootItem item, Transform slot)
    {
        GameObject icon = new GameObject("Placeholder_" + item.Name, typeof(RectTransform));
        icon.transform.SetParent(slot, false);
        RectTransform rt = icon.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(3, 3);
        rt.offsetMax = new Vector2(-3, -3);
        Image img = icon.AddComponent<Image>();
        img.color = new Color(0.35f, 0.45f, 0.7f);

        GameObject txt = new GameObject("Name", typeof(RectTransform));
        txt.transform.SetParent(icon.transform, false);
        RectTransform trt = txt.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        TextMeshProUGUI t = txt.AddComponent<TextMeshProUGUI>();
        t.text = item.Name;
        t.fontSize = 8;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        return icon;
    }
}

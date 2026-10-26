using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro;

public class UI_Controler : MonoBehaviour
{
    public Player_Inventory PlayerInventory;

    public GameObject InventoryPannel;
    public GameObject EquipmentPannel;
    public GameObject AtributtesPannel;

    public GameObject InventoryButton;
    public GameObject EquipmentButton;
    public GameObject AtributtesButton;

    public TMP_Text InventoryRuples;
    public TMP_Text InventoryMaxHP;
    public TMP_Text InventoryHealth;
    public TMP_Text InventoryDamage;
    public TMP_Text InventoryArmour;
    public TMP_Text InventorySpeed;

    public UnityEvent OnChangedRuples;

    public GameObject Tooltip;
    private PlayerStats _stats;

    private void Start()
    {
        OnChangedRuples.AddListener(_changeRuple);
        OnChangedRuples.Invoke();

        //atributi se sami osvjeze kad se stat promijeni
        _stats = GameObject.Find("Player").GetComponent<PlayerStats>();
        _stats.Health.CurrentChanged += _changeHealth;
        _stats.Armour.Changed += value => InventoryArmour.text = Mathf.RoundToInt(value).ToString();
        _stats.Damage.Changed += value => InventoryDamage.text = Mathf.RoundToInt(value).ToString();
        _stats.Speed.Changed += value => InventorySpeed.text = Mathf.RoundToInt(value).ToString();

        _changeHealth(_stats.Health.Current, _stats.Health.Max);
        InventoryArmour.text = Mathf.RoundToInt(_stats.Armour.Value).ToString();
        InventoryDamage.text = Mathf.RoundToInt(_stats.Damage.Value).ToString();
        InventorySpeed.text = Mathf.RoundToInt(_stats.Speed.Value).ToString();
    }

    private void OnDestroy()
    {
        //UI moze nestati prije playera, pa se odjavi da event ne zove unisteni objekt
        if (_stats != null) _stats.Health.CurrentChanged -= _changeHealth;
    }

    private void _changeHealth(float current, float max)
    {
        InventoryHealth.text = Mathf.RoundToInt(current).ToString();
        InventoryMaxHP.text = Mathf.RoundToInt(max).ToString();
    }
    private void _changeRuple()
    {
        InventoryRuples.text = Player_Controler.RupleCurrency.ToString();
    }

    void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame){
            InventoryActivitySwap();
        }
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            EquipmentActivitySwap();
        }
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            AtributtesActivitySwap();
        }
    }
    public void InventoryActivitySwap()
    {
        SFX.Play(SFX.Instance.OpenWindow); //zvuk otvaranja/zatvaranja
        if (InventoryPannel.active)
        {
            InventoryPannel.SetActive(false);
            InventoryButton.SetActive(true);
        }
        else
        {
            InventoryPannel.SetActive(true);
            InventoryButton.SetActive(false);
        }
    }
    public void EquipmentActivitySwap()
    {
        SFX.Play(SFX.Instance.OpenWindow); //zvuk otvaranja/zatvaranja
        if (EquipmentPannel.active)
        {
            EquipmentPannel.SetActive(false);
            EquipmentButton.SetActive(true);
        }
        else
        {
            EquipmentPannel.SetActive(true);
            EquipmentButton.SetActive(false);
        }
    }
    public void AtributtesActivitySwap()
    {
        SFX.Play(SFX.Instance.OpenWindow); //zvuk otvaranja/zatvaranja
        if (AtributtesPannel.active)
        {
            AtributtesPannel.SetActive(false);
            AtributtesButton.SetActive(true);
        }
        else
        {
            AtributtesPannel.SetActive(true);
            AtributtesButton.SetActive(false);
        }
    }
    public void ToolTipActive()
    {
        Tooltip.SetActive(true);
    }
}

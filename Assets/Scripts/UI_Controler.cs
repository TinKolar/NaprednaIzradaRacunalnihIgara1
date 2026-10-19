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

    public UnityEvent OnChangedHealth;
    public UnityEvent OnChangedMaxHP;
    public UnityEvent OnChangedRuples;
    public UnityEvent OnChangedDamage;
    public UnityEvent OnChangedArmour;
    public UnityEvent OnChangedSpeed;

    public Animator PlayerAnimator;
    public GameObject Tooltip;
    private void Start()
    {
        OnChangedRuples.AddListener(_changeRuple);
        OnChangedMaxHP.AddListener(_changeMaxHP);
        OnChangedHealth.AddListener(_changeHealth);
        OnChangedDamage.AddListener(_changeDamage);
        OnChangedArmour.AddListener(_changeArmour);
        OnChangedSpeed.AddListener(_changeSpeed);

        OnChangedMaxHP.Invoke();
        OnChangedHealth.Invoke();
        OnChangedRuples.Invoke();
        OnChangedDamage.Invoke();
        OnChangedArmour.Invoke();
        OnChangedSpeed.Invoke();       
    }
  
    private void _changeHealth()
    {        
        if(Player_Controler.CurrentHealth > Player_Controler.MaxHealth)
        {
            Player_Controler.CurrentHealth = Player_Controler.MaxHealth;
        }
        InventoryHealth.text = Player_Controler.CurrentHealth.ToString();
    }
    private void _changeMaxHP()
    {
        InventoryMaxHP.text = Player_Controler.MaxHealth.ToString();
    }
    private void _changeRuple()
    {
        InventoryRuples.text = Player_Controler.RupleCurrency.ToString();
    }
    private void _changeDamage()
    {
        //osnovna steta + steta oruzja
        InventoryDamage.text = Player_Controler.Damage.ToString();
    }
    private void _changeArmour()
    {
        InventoryArmour.text = Player_Controler.Armour.ToString();
    }
    private void _changeSpeed()
    {
        InventorySpeed.text = Player_Controler.Speed.ToString();
        PlayerAnimator.speed = Player_Controler.Speed;
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

using UnityEngine;
using UnityEngine.UI;
using TMPro;

//HUD gore lijevo: health, stamina i ruple — osvjezava se samo kad se vrijednost promijeni
public class HUD_Status : MonoBehaviour
{
    public Image HealthFill;
    public TextMeshProUGUI HealthText;
    public Image StaminaFill;
    public TextMeshProUGUI StaminaText;
    public TextMeshProUGUI RupleText;

    void OnEnable()
    {
        Player_Controler.OnHealthChanged += RefreshHealth;
        Player_Controler.OnStaminaChanged += RefreshStamina;
        Player_Controler.OnRuplesChanged += RefreshRuples;
        //pocetno stanje, vrijednosti su se mozda promijenile prije pretplate
        RefreshHealth();
        RefreshStamina();
        RefreshRuples();
    }

    void OnDisable()
    {
        Player_Controler.OnHealthChanged -= RefreshHealth;
        Player_Controler.OnStaminaChanged -= RefreshStamina;
        Player_Controler.OnRuplesChanged -= RefreshRuples;
    }

    void RefreshHealth()
    {
        HealthFill.fillAmount = (float)Player_Controler.CurrentHealth / Player_Controler.MaxHealth;
        HealthText.text = Player_Controler.CurrentHealth + " / " + Player_Controler.MaxHealth;
    }

    void RefreshStamina()
    {
        StaminaFill.fillAmount = Player_Controler.CurrentStamina / Player_Controler.MaxStamina;
        StaminaText.text = (int)Player_Controler.CurrentStamina + " / " + Player_Controler.MaxStamina;
    }

    void RefreshRuples()
    {
        RupleText.text = Player_Controler.RupleCurrency.ToString();
    }
}

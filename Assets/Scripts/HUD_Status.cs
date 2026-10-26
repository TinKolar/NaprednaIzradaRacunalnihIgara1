using UnityEngine;
using UnityEngine.UI;
using TMPro;

//HUD gore lijevo: health, stamina i ruple
public class HUD_Status : MonoBehaviour
{
    public Image HealthFill;
    public TextMeshProUGUI HealthText;
    public Image StaminaFill;
    public TextMeshProUGUI StaminaText;
    public TextMeshProUGUI RupleText;

    private PlayerStats _stats;

    void Start()
    {
        _stats = GameObject.Find("Player").GetComponent<PlayerStats>();
        _stats.Health.CurrentChanged += ShowHealth;
        _stats.Stamina.CurrentChanged += ShowStamina;
        ShowHealth(_stats.Health.Current, _stats.Health.Max);
        ShowStamina(_stats.Stamina.Current, _stats.Stamina.Max);
    }

    void OnDestroy()
    {
        if (_stats == null) return;
        _stats.Health.CurrentChanged -= ShowHealth;
        _stats.Stamina.CurrentChanged -= ShowStamina;
    }

    void ShowHealth(float current, float max)
    {
        HealthFill.fillAmount = current / max;
        HealthText.text = Mathf.RoundToInt(current) + " / " + Mathf.RoundToInt(max);
    }

    void ShowStamina(float current, float max)
    {
        StaminaFill.fillAmount = current / max;
        StaminaText.text = (int)current + " / " + Mathf.RoundToInt(max);
    }

    void Update()
    {
        //ruple jos nemaju event
        RupleText.text = Player_Controler.RupleCurrency.ToString();
    }
}

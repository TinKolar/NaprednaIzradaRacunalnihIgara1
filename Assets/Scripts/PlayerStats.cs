using System.Collections.Generic;
using UnityEngine;

//Svi statsi playera na jednom mjestu. Osnovne vrijednosti se namjestaju u Inspectoru,
//oprema dodaje modifiere preko ApplyEquipment, a UI se pretplati na Changed / CurrentChanged evente.
public class PlayerStats : MonoBehaviour
{
    [Header("Osnovne vrijednosti (bez opreme)")]
    public float BaseHealth = 100;
    public float BaseStamina = 100;
    public float BaseArmour = 1;
    public float BaseDamage = 1;
    public float BaseSpeed = 3;

    public Resource Health { get; private set; }
    public Resource Stamina { get; private set; }
    public Stat Armour { get; private set; }
    public Stat Damage { get; private set; }
    public Stat Speed { get; private set; }

    private Dictionary<StatType, Stat> _stats;
    private List<object> _equipmentSources = new List<object>();

    void Awake()
    {
        Health = new Resource(StatType.Health, BaseHealth);
        Stamina = new Resource(StatType.Stamina, BaseStamina);
        Armour = new Stat(StatType.Armour, BaseArmour);
        Damage = new Stat(StatType.Damage, BaseDamage);
        Speed = new Stat(StatType.Speed, BaseSpeed);

        _stats = new Dictionary<StatType, Stat>
        {
            { StatType.Health, Health },
            { StatType.Stamina, Stamina },
            { StatType.Armour, Armour },
            { StatType.Damage, Damage },
            { StatType.Speed, Speed },
        };
    }

    public Stat Get(StatType type)
    {
        return _stats[type];
    }

    //Makne bonuse stare opreme i doda bonuse trenutno equipane (source = item u slotu)
    public void ApplyEquipment(IEnumerable<(object source, EquippableData data)> equipped)
    {
        foreach (object source in _equipmentSources)
        {
            foreach (Stat stat in _stats.Values)
            {
                stat.RemoveFrom(source);
            }
        }
        _equipmentSources.Clear();

        foreach (var (source, data) in equipped)
        {
            if (data.MaxHealth != 0) Health.AddModifier(source, data.MaxHealth);
            if (data.Armour != 0) Armour.AddModifier(source, data.Armour);
            if (data.Damage != 0) Damage.AddModifier(source, data.Damage);
            if (data.Speed != 0) Speed.AddModifier(source, data.Speed);
            _equipmentSources.Add(source);
        }
    }
}

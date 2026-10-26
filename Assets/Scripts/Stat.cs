using System.Collections.Generic;
using System;

//Jedan stat (npr. Armour). Value = BaseValue + svi modifieri (oprema, buffovi...).
//Modifier se veze uz izvor (source) da se moze maknuti kad se izvor makne (unequip).
public class Stat
{
    public StatType Type { get; private set; }
    public float Value { get; private set; }
    public float BaseValue { get; private set; }

    public event Action<float> Changed;

    private List<(object source, float value)> _mods = new();

    public Stat(StatType type, float baseValue)
    {
        Type = type;
        BaseValue = baseValue;
        Value = baseValue;
    }

    public void AddModifier(object source, float value)
    {
        _mods.Add((source, value));
        Recalculate();
    }

    public void RemoveFrom(object source)
    {
        if (_mods.RemoveAll(m => m.source == source) > 0)
        {
            Recalculate();
        }
    }

    private void Recalculate()
    {
        float value = BaseValue;
        foreach (var mod in _mods)
        {
            value += mod.value;
        }
        Value = value;
        Changed?.Invoke(value);
    }
}

//Enumi se spremaju kao broj! Nove vrijednosti dodavati samo na kraj.
public enum StatType
{
    Health = 0,
    Stamina = 1,
    Armour = 2,
    Damage = 3,
    Speed = 4
}

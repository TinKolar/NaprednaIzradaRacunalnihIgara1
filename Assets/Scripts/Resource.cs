using System;
using UnityEngine;

//Stat koji se trosi i puni (Health, Stamina). Max = Value iz Stata (osnova + oprema), Current = trenutno stanje.
public class Resource : Stat
{
    public float Max => Value;
    public float Current { get; private set; }
    public event Action<float, float> CurrentChanged; // (current, max)
    public event Action Depleted;

    public Resource(StatType type, float baseValue) : base(type, baseValue)
    {
        Current = Max; //krece pun
        //kad se Max promijeni (npr. skine se oprema s +HP) Current ne smije biti veci od Max
        Changed += _ => SetCurrent(Current, true);
    }

    public void Increase(float amount)
    {
        SetCurrent(Current + amount);
    }

    public void Decrease(float amount)
    {
        SetCurrent(Current - amount);
    }

    public void Refill()
    {
        SetCurrent(Max);
    }

    public bool TryConsume(float amount)
    {
        if (Current < amount)
        {
            return false;
        }
        Decrease(amount);
        return true;
    }

    private void SetCurrent(float value, bool forceNotify = false)
    {
        float v = Mathf.Clamp(value, 0, Max);
        if (v == Current && !forceNotify)
        {
            return;
        }
        bool wasAlive = Current > 0;
        Current = v;
        CurrentChanged?.Invoke(Current, Max);
        if (Current <= 0 && wasAlive)
        {
            Depleted?.Invoke();
        }
    }
}

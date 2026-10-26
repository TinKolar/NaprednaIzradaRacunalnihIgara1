using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LootItem
{
    public ItemData Item;            // oprema, oruzje ili potion (null + Ruples = novac)
    public int Amount = 1;
    public bool Ruples;              // novac nije item, ide ravno u RupleCurrency

    public string Name
    {
        get
        {
            if (Ruples) return "Ruples";
            return Item != null ? Item.ItemName : "?";
        }
    }
}

public class Loot_Bag : MonoBehaviour
{
    public List<LootItem> Items = new List<LootItem>();
    public bool DestroyWhenEmpty = true; // skrinja ostaje, vreca nestane

    private bool glowOff = false;

    //poklopac skrinje (samo za skrinju) - otvori se kad je prazna
    public Transform Lid;
    public float LidOpenAngle = -110f;
    public float LidOpenSpeed = 3f;
    private float lidT = 0;

    void Update()
    {
        if (Lid != null && Items.Count == 0 && lidT < 1)
        {
            lidT = Mathf.Min(1, lidT + Time.deltaTime * LidOpenSpeed);
            //malo odskoci na kraju
            float t = 1 - Mathf.Pow(1 - lidT, 3);
            Lid.localEulerAngles = new Vector3(LidOpenAngle * t, 0, 0);
        }
        if (Items.Count == 0 && DestroyWhenEmpty)
        {
            Destroy(gameObject);
        }
        //prazna skrinja ostaje ali vise ne svijetli
        if (Items.Count == 0 && DestroyWhenEmpty == false && glowOff == false)
        {
            glowOff = true;
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                if (r is ParticleSystemRenderer) continue;
                if (r.sharedMaterials.Length > 1)
                {
                    r.sharedMaterials = new Material[] { r.sharedMaterials[0] };
                }
            }
            foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>())
            {
                ps.Stop();
            }
        }
    }
}

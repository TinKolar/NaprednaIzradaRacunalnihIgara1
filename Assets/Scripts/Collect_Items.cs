using UnityEngine;
using UnityEngine.InputSystem;

//Na playeru: automatski kupi srca i ruple, a kod loot vrece/skrinje F otvara loot prozor
public class Collect_Items : MonoBehaviour
{
    public UI_Controler UiControler;
    public Loot_Window LootWindow;
    private GameObject _lootForPickup;

    private void Update()
    {
        //Otvori loot prozor na F kad je player kraj vrece
        if (_lootForPickup != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            LootWindow.Open(_lootForPickup.GetComponent<Loot_Bag>());
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        //Pokupi Heart i doda vrjednost
        if (collision.tag == "Heart")
        {
            GetComponent<PlayerStats>().Health.Increase(20);
            SFX.Play(SFX.Instance.DrinkPotion, 0.6f);
            Destroy(collision.gameObject);
            Debug.Log("Collected: " + collision.name);
        }
        //Pokupi Ruple i doda vrjednost
        if (collision.tag == "Ruple")
        {
            Player_Controler.RupleCurrency += collision.GetComponent<Ruple_Value>().RupleValue;
            UiControler.OnChangedRuples.Invoke();
            SFX.Play(SFX.Instance.Coins);
            Destroy(collision.gameObject);
            Debug.Log("Collected: " + collision.name);
        }
        if (collision.tag == "Loot")
        {
            _lootForPickup = collision.gameObject;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.tag == "Loot")
        {
            _lootForPickup = null;
        }
    }
}

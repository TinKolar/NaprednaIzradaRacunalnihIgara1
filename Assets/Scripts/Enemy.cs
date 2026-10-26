using UnityEngine;
using UnityEngine.AI;
using TheKiwiCoder;

//Neprijatelj: health, primanje stete (Player_Controler salje SendMessage("TakeDamage")), animacije i smrt
public class Enemy : MonoBehaviour , IDamagable
{
    public int MaxHealth = 30;
    public int Health = 30;
    public Animator animator;

    public EnemyHealthBar HealthBar;

    [Header("Loot drop")]
    public GameObject LootBagPrefab;
    public int MinRuples = 5;
    public int MaxRuples = 20;
    public System.Collections.Generic.List<WeaponData> WeaponDrops = new System.Collections.Generic.List<WeaponData>();
    [Range(0, 1)] public float WeaponDropChance = 0.35f;
    [Range(0, 1)] public float PotionDropChance = 0.5f;
    public ConsumableData PotionDrop;
    public System.Collections.Generic.List<GearData> GearDrops = new System.Collections.Generic.List<GearData>();
    [Range(0, 1)] public float GearDropChance = 0.3f;

    void Start()
    {
        Health = MaxHealth;
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        gameObject.tag = "Enemy";
        if (animator != null)
        {
            animator.applyRootMotion = false; //kretanje vozi NavMeshAgent
        }
        agent = GetComponent<NavMeshAgent>();
    }

    private NavMeshAgent agent;
    public float RunAnimationSpeed = 3f; //brzina pri kojoj je Running = 1

    void Update()
    {
        if (animator != null && agent != null && agent.enabled && HasParameter("Running"))
        {
            animator.SetFloat("Running", agent.velocity.magnitude / RunAnimationSpeed, 0.1f, Time.deltaTime);
        }
        if (Health <= 0) return;

        //koraci
        if (agent != null && agent.enabled && agent.velocity.magnitude > 0.3f)
        {
            stepCounter = stepCounter + agent.velocity.magnitude * Time.deltaTime;
            if (stepCounter >= StepDistance)
            {
                stepCounter = 0;
                SFX.PlayAt(SFX.Instance.Footsteps, transform.position, 0.35f, 0.2f);
            }
        }

        //zombi povremeno zareze
        growlTimer = growlTimer - Time.deltaTime;
        if (growlTimer <= 0)
        {
            growlTimer = Random.Range(MinGrowlTime, MaxGrowlTime);
            SFX.PlayAt(SFX.Instance.ZombieGrowl, transform.position, 0.5f, 0.15f);
        }
    }

    public float StepDistance = 1.2f;
    private float stepCounter = 0;
    public float MinGrowlTime = 6;
    public float MaxGrowlTime = 14;
    private float growlTimer = 3;
    private float lastAggro = -100;

    //zove SearchingForEnemy kad ugleda playera
    public void Spotted()
    {
        if (Time.time - lastAggro > 10)
        {
            SFX.PlayAt(SFX.Instance.ZombieAggro, transform.position, 0.8f);
            growlTimer = Random.Range(MinGrowlTime, MaxGrowlTime);
        }
        lastAggro = Time.time;
    }

    public void TakeDamage(int amount)
    {
        if (Health <= 0) return;
        Health = Health - amount;
        Debug.Log(name + " took " + amount + " damage, health " + Health);
        //health bar se pokaze tek kad primi stetu + broj stete iznad glave
        if (HealthBar != null)
        {
            HealthBar.gameObject.SetActive(true);
        }
        DamageNumber.Spawn(transform.position + Vector3.up * 2.3f, amount, new Color(1f, 0.85f, 0.3f));
        SFX.PlayAt(SFX.Instance.EnemyHit, transform.position);
        if (Health <= 0)
        {
            Health = 0;
            Die();
        }
    }

    public void PlayAttack()
    {
        if (animator != null && HasParameter("Attack"))
        {
            animator.SetTrigger("Attack");
        }
        SFX.PlayAt(SFX.Instance.ZombieAttack, transform.position, 0.8f);
    }

    void Die()
    {
        if (animator != null && HasParameter("Death"))
        {
            animator.SetBool("Death", true);
        }
        StopAllCoroutines();
        SFX.PlayAt(SFX.Instance.ZombieDeath, transform.position);
        DropLoot();
        //ugasi AI i kretanje
        BehaviourTreeRunner runner = GetComponent<BehaviourTreeRunner>();
        if (runner != null) runner.enabled = false;
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        Destroy(gameObject, 3);
    }

    void DropLoot()
    {
        if (LootBagPrefab == null) return;
        //spusti vrecu na tlo
        Vector3 pos = transform.position + transform.right * 0.8f;
        RaycastHit hit;
        if (Physics.Raycast(pos + Vector3.up * 2f, Vector3.down, out hit, 10f, ~0, QueryTriggerInteraction.Ignore))
        {
            pos = hit.point;
        }
        GameObject bag = Instantiate(LootBagPrefab, pos, Quaternion.Euler(0, Random.Range(0, 360), 0));
        Loot_Bag loot = bag.GetComponent<Loot_Bag>();
        loot.Items.Clear();
        loot.Items.Add(new LootItem { Ruples = true, Amount = Random.Range(MinRuples, MaxRuples + 1) });
        if (PotionDrop != null && Random.value < PotionDropChance)
        {
            loot.Items.Add(new LootItem { Item = PotionDrop, Amount = Random.Range(1, 3) });
        }
        if (GearDrops.Count > 0 && Random.value < GearDropChance)
        {
            GearData g = GearDrops[Random.Range(0, GearDrops.Count)];
            loot.Items.Add(new LootItem { Item = g });
        }
        if (WeaponDrops.Count > 0 && Random.value < WeaponDropChance)
        {
            WeaponData w = WeaponDrops[Random.Range(0, WeaponDrops.Count)];
            loot.Items.Add(new LootItem { Item = w });
        }
    }

    bool HasParameter(string paramName)
    {
        foreach (AnimatorControllerParameter p in animator.parameters)
        {
            if (p.name == paramName) return true;
        }
        return false;
    }
}

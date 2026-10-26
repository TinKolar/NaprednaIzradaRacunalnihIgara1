using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PlayerStats))]
public class Player_Controler : MonoBehaviour
{
    public static int RupleCurrency { get; set; } = 0;
    public static int InventoryCapacity { get; set; } = 32;

    //Health, Stamina, Armour, Damage, Speed (osnova + oprema) su u PlayerStats komponenti
    public PlayerStats Stats { get; private set; }

    //Stamina se trosi na napade i puni se kad player ne napada
    public float StaminaRegen = 20; //po sekundi
    public float LightAttackStamina = 15;
    public float HeavyAttackStamina = 40;
    //Korigira brzinu kod dijagonalnog kretanja => cos 45' = 0.7071 ili korijen iz 2 / 2 =  0.7071.
    public const float DIAGONAL_MOVEMENT_CORRECTION = 0.7071f;

    private NavMeshAgent _playerNavMeshAgent;
    private Animator _playerAnimator;

    private float movementSpeed;
    private bool isMovingWithKeys = false;
    private Vector3 direction;

    public bool isAttacking = false;
    private float attackTimer = 0;
    private Quaternion attackRotation;
    public float attackTurnSpeed = 540; //stupnjeva u sekundi

    [Header("Weapon")]
    public WeaponData CurrentWeapon;
    //clipovi iz Player.controllera koje oruzje zamjenjuje
    public AnimationClip LightAttackPlaceholder;
    public AnimationClip HeavyAttackPlaceholder;
    private AnimatorOverrideController _overrideController;
    public Transform WeaponSocket; // child od mixamorig:RightHand
    public GameObject DefaultHeavyVFX;
    public float HeavyVFXLead = 0.4f; //koliko sekundi prije udarca se pali slash efekt
    private GameObject _weaponModel;
    public Transform OffHandSocket; //ako je prazno uzme se lijeva ruka iz animatora
    private GameObject _offHandModel;

    void Awake()
    {
        Stats = GetComponent<PlayerStats>();
    }

    void Start()
    {
        _playerNavMeshAgent = this.GetComponent<NavMeshAgent>();
        Stats.Health.Depleted += Die;
        _playerAnimator = this.GetComponent<Animator>();
        _playerAnimator.speed = 1;
        _playerAnimator.applyRootMotion = false;

        // inspector vrijednosti su prespore za ovo
        _playerNavMeshAgent.angularSpeed = 720;
        _playerNavMeshAgent.acceleration = 30;
        _playerNavMeshAgent.stoppingDistance = 0.1f;

        //jedna instanca override controllera, oruzja u njoj samo mijenjaju clipove
        _overrideController = new AnimatorOverrideController(_playerAnimator.runtimeAnimatorController);
        _playerAnimator.runtimeAnimatorController = _overrideController;
        EquipWeapon(CurrentWeapon);

        //pocetno oruzje stavi i u weapon slot da se moze skinut/zamijenit
        GameObject gm = GameObject.Find("GameManager");
        if (CurrentWeapon != null && gm != null)
        {
            Player_Inventory inv = gm.GetComponent<Player_Inventory>();
            if (inv != null && inv.WeaponSlot.transform.childCount == 0)
            {
                Inventory_LootItem.Create(CurrentWeapon, 1, inv.WeaponSlot.transform);
            }
        }
        RefreshEquipment(); //bonusi pocetnog oruzja
        Stats.Health.Refill();
        Stats.Stamina.Refill();
    }

    //Zamjeni animacije napada i brzinu napada prema oruzju (null = bez oruzja)
    public void EquipWeapon(WeaponData weapon)
    {
        CurrentWeapon = weapon;
        AnimationClip light = null;
        AnimationClip heavy = null;
        float speed = 1;
        if (weapon != null)
        {
            light = weapon.LightAttack;
            heavy = weapon.HeavyAttack;
            speed = weapon.AttackSpeed;
        }
        //null u overrideu = vraca originalni clip iz controllera
        _overrideController[LightAttackPlaceholder] = light;
        _overrideController[HeavyAttackPlaceholder] = heavy;
        _playerAnimator.SetFloat("AttackSpeed", speed);

        //model oruzja u ruci
        if (_weaponModel != null)
        {
            Destroy(_weaponModel);
        }
        if (weapon != null && weapon.Model != null && WeaponSocket != null)
        {
            _weaponModel = Instantiate(weapon.Model, WeaponSocket);
            _weaponModel.transform.localPosition = weapon.HoldOffset;
            _weaponModel.transform.localRotation = Quaternion.Euler(weapon.HoldRotation);
            foreach (Collider c in _weaponModel.GetComponentsInChildren<Collider>())
            {
                Destroy(c);
            }
        }
        Debug.Log("Equipped " + (weapon != null ? weapon.ItemName : "nothing"));
    }

    //Zove se nakon svakog equip/unequip: oruzje u main handu, model u off handu i statsi
    public void RefreshEquipment()
    {
        Player_Inventory inv = GameObject.Find("GameManager").GetComponent<Player_Inventory>();
        Inventory_LootItem main = inv.WeaponSlot.GetComponent<EquipSlot>().Current;
        Inventory_LootItem off = inv.ShieldSlot.GetComponent<EquipSlot>().Current;
        WeaponData mainWeapon = main != null ? main.Data as WeaponData : null;
        if (mainWeapon != CurrentWeapon) EquipWeapon(mainWeapon);

        //off hand (drugo oruzje) samo vizualno + statsi, napadi idu s main hand oruzjem
        if (_offHandModel != null) Destroy(_offHandModel);
        if (OffHandSocket == null) OffHandSocket = _playerAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
        WeaponData offWeapon = off != null ? off.Data as WeaponData : null;
        if (offWeapon != null && offWeapon.Model != null && OffHandSocket != null)
        {
            _offHandModel = Instantiate(offWeapon.Model, OffHandSocket);
            _offHandModel.transform.localPosition = offWeapon.HoldOffset;
            _offHandModel.transform.localRotation = Quaternion.Euler(offWeapon.HoldRotation);
            foreach (Collider c in _offHandModel.GetComponentsInChildren<Collider>()) Destroy(c);
        }

        //bonusi sve equipane opreme (i oruzja) idu u statse kao modifieri
        List<(object, EquippableData)> equipped = new List<(object, EquippableData)>();
        foreach (EquipSlot slot in inv.EquipSlots())
        {
            Inventory_LootItem item = slot.Current;
            if (item != null && item.Data is EquippableData) equipped.Add((item, (EquippableData)item.Data));
        }
        Stats.ApplyEquipment(equipped);
    }

    private AnimationClip currentLightClip()
    {
        if (CurrentWeapon != null && CurrentWeapon.LightAttack != null) return CurrentWeapon.LightAttack;
        return LightAttackPlaceholder;
    }

    private AnimationClip currentHeavyClip()
    {
        if (CurrentWeapon != null && CurrentWeapon.HeavyAttack != null) return CurrentWeapon.HeavyAttack;
        return HeavyAttackPlaceholder;
    }

    private float attackSpeed()
    {
        if (CurrentWeapon != null && CurrentWeapon.AttackSpeed > 0) return CurrentWeapon.AttackSpeed;
        return 1;
    }

    private int weaponDamage()
    {
        return Mathf.RoundToInt(Stats.Damage.Value); //damage oruzja je vec u Damage (modifier)
    }


    public bool isDead = false;

    void Update()
    {
        //smrt ide preko Stats.Health.Depleted eventa
        if (isDead == true)
        {
            return;
        }

        _playerAttack();
        if (isAttacking == false)
        {
            Stats.Stamina.Increase(StaminaRegen * Time.deltaTime);
            _playerMovementControl();
            _playerMouseMovement();
        }
        _playerAnimationHandler();
    }

    //Je li mis iznad nekog UI elementa (gumb, prozor, slot...)
    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        isAttacking = false;
        CancelInvoke(); //da ne udari nakon smrti
        _playerNavMeshAgent.ResetPath();
        _playerNavMeshAgent.velocity = Vector3.zero;
        _playerNavMeshAgent.isStopped = true;
        _playerAnimator.SetFloat("RunBlend", 0);
        _playerAnimator.SetBool("Death", true);
        SFX.PlayAt(SFX.Instance.PlayerDeath, transform.position);
        //mrtav player vise ne kupi ruple i srca
        SphereCollider pickup = GetComponent<SphereCollider>();
        if (pickup != null)
        {
            pickup.enabled = false;
        }
        Debug.Log("Player died");
    }

    //Napadi: desni klik = normalni napad, Q = AOE napad
    private void _playerAttack()
    {
        if (isAttacking == true)
        {
            //polako se okreni prema smjeru napada
            transform.rotation = Quaternion.RotateTowards(transform.rotation, attackRotation, attackTurnSpeed * Time.deltaTime);
            attackTimer = attackTimer - Time.deltaTime;
            if (attackTimer <= 0)
            {
                isAttacking = false;
            }
            return;
        }

        //klik po UI (inventory, loot...) ne smije napasti
        if (Mouse.current.rightButton.wasPressedThisFrame && IsPointerOverUI())
        {
            //nista
        }
        else if (Mouse.current.rightButton.wasPressedThisFrame && !Stats.Stamina.TryConsume(LightAttackStamina))
        {
            Debug.Log("Not enough stamina!");
        }
        else if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            isAttacking = true;
            //trajanje i trenutak udarca dolaze iz clipa i oruzja
            attackTimer = currentLightClip().length / attackSpeed();
            isMovingWithKeys = false;
            _playerNavMeshAgent.ResetPath();
            _playerNavMeshAgent.velocity = Vector3.zero;
            //okreni se prema misu (rotacija se radi postepeno gore u isAttacking)
            attackRotation = transform.rotation;
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000, ~0, QueryTriggerInteraction.Ignore))
            {
                Vector3 lookAt = hit.point - transform.position;
                lookAt.y = 0;
                if (lookAt != Vector3.zero)
                {
                    attackRotation = Quaternion.LookRotation(lookAt);
                }
            }
            _playerAnimator.SetInteger("AttackType", 2);
            _playerAnimator.SetTrigger("Attack");
            float lightHit = CurrentWeapon != null ? CurrentWeapon.LightHitTime : 0.4f;
            Invoke("SingleTargetHit", attackTimer * lightHit); //kad mac udari
            Invoke("LightSwingSound", Mathf.Max(0, attackTimer * lightHit - 0.15f));
        }

        if (Keyboard.current.qKey.wasPressedThisFrame && !Stats.Stamina.TryConsume(HeavyAttackStamina))
        {
            Debug.Log("Not enough stamina!");
        }
        else if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            isAttacking = true;
            attackTimer = currentHeavyClip().length / attackSpeed();
            isMovingWithKeys = false;
            _playerNavMeshAgent.ResetPath();
            _playerNavMeshAgent.velocity = Vector3.zero;
            attackRotation = transform.rotation; //AOE se ne okrece
            _playerAnimator.SetInteger("AttackType", 1);
            _playerAnimator.SetTrigger("Attack");
            float heavyHit = CurrentWeapon != null ? CurrentWeapon.HeavyHitTime : 0.45f;
            Invoke("AoeHit", attackTimer * heavyHit);
            Invoke("SpawnHeavyVFX", Mathf.Max(0, attackTimer * heavyHit - HeavyVFXLead));
            Invoke("HeavySwingSound", Mathf.Max(0, attackTimer * heavyHit - HeavyVFXLead));
        }
    }

    private void LightSwingSound()
    {
        SFX.PlayAt(SFX.Instance.LightSwing, transform.position);
    }

    private void HeavySwingSound()
    {
        SFX.PlayAt(SFX.Instance.HeavySwing, transform.position);
    }

    private void SingleTargetHit()
    {
        float range = CurrentWeapon != null ? CurrentWeapon.LightRange : 1.5f;
        float radius = CurrentWeapon != null ? CurrentWeapon.LightRadius : 1.2f;
        int dmg = weaponDamage();
        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * range, radius);
        GameObject closest = null;
        float closestDistance = 999;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].gameObject.tag == "Enemy")
            {
                float d = Vector3.Distance(transform.position, hits[i].transform.position);
                if (d < closestDistance)
                {
                    closestDistance = d;
                    closest = hits[i].gameObject;
                }
            }
        }
        if (closest != null)
        {
            closest.SendMessage("TakeDamage", dmg, SendMessageOptions.DontRequireReceiver);
            Debug.Log("Hit " + closest.name + " for " + dmg);
        }
    }

    //slash efekt jakog napada (pali se malo prije udarca da se poklopi s animacijom)
    private void SpawnHeavyVFX()
    {
        if (isDead) return;
        GameObject vfx = DefaultHeavyVFX;
        if (CurrentWeapon != null && CurrentWeapon.HeavyAttackVFX != null)
        {
            vfx = CurrentWeapon.HeavyAttackVFX;
        }
        if (vfx != null)
        {
            GameObject fx = Instantiate(vfx, transform.position + Vector3.up * 1f, transform.rotation);
            Destroy(fx, 2);
        }
    }

    private void AoeHit()
    {
        float radius = CurrentWeapon != null ? CurrentWeapon.HeavyRadius : 3.5f;
        int dmg = CurrentWeapon != null ? weaponDamage() * CurrentWeapon.HeavyDamageMultiplier : weaponDamage();
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].gameObject.tag == "Enemy")
            {
                hits[i].gameObject.SendMessage("TakeDamage", dmg, SendMessageOptions.DontRequireReceiver);
                Debug.Log("AOE hit " + hits[i].gameObject.name + " for " + dmg);
            }
        }
    }

    private void _playerMovementControl()
    {
        movementSpeed = Stats.Speed.Value * 1.5f;
        //Shift = hodanje (prije je bilo trcanje)
        if (Keyboard.current.leftShiftKey.isPressed)
        {
            movementSpeed = Stats.Speed.Value * 0.5f;
        }
        _playerNavMeshAgent.speed = movementSpeed;

        direction = new Vector3(0, 0, 0);
        //Down
        if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed)
        {
            direction = direction + new Vector3(0, 0, -1);
        }
        //Up
        if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed)
        {
            direction = direction + new Vector3(0, 0, 1);
        }
        //Left
        if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
        {
            direction = direction + new Vector3(-1, 0, 0);
        }
        //Right
        if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
        {
            direction = direction + new Vector3(1, 0, 0);
        }
        //Diagonalno kretanje
        if (direction.x != 0 && direction.z != 0)
        {
            direction = direction * DIAGONAL_MOVEMENT_CORRECTION;
        }

        if (direction != Vector3.zero)
        {
            isMovingWithKeys = true;
            //tipke prekidaju klik kretanje
            _playerNavMeshAgent.ResetPath();
            //Move ne da playeru izac sa navmesha
            _playerNavMeshAgent.Move(direction * movementSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 15 * Time.deltaTime);
        }
        else
        {
            isMovingWithKeys = false;
        }
    }

    public float LootClickRange = 2.5f;
    public float StepDistance = 1.4f;
    private float stepCounter = 0;
    private bool holdingLootClick = false;

    //Kretanje misem kao u diablu
    private void _playerMouseMovement()
    {
        if (isMovingWithKeys == true)
        {
            return;
        }
        //drzanje lijeve tipke = player ide za misem
        //klik po UI ne smije pomaknuti playera
        //klik na vrecu/skrinju - ako je blizu otvori loot prozor, inace hodaj do nje
        if (Mouse.current.leftButton.wasPressedThisFrame && IsPointerOverUI() == false)
        {
            Ray lootRay = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit[] hits = Physics.RaycastAll(lootRay, 1000, ~0, QueryTriggerInteraction.Collide);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (RaycastHit h in hits)
            {
                Loot_Bag bag = h.collider.GetComponentInParent<Loot_Bag>();
                if (bag == null) continue;
                if (bag.Items.Count > 0 && Vector3.Distance(transform.position, bag.transform.position) <= LootClickRange)
                {
                    FindFirstObjectByType<Loot_Window>().Open(bag);
                    _playerNavMeshAgent.ResetPath();
                    holdingLootClick = true;
                }
                break;
            }
        }
        if (Mouse.current.leftButton.isPressed == false)
        {
            holdingLootClick = false;
        }
        if (holdingLootClick == true)
        {
            return;
        }
        if (Mouse.current.leftButton.isPressed && IsPointerOverUI() == false)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000, ~0, QueryTriggerInteraction.Ignore))
            {
                _playerNavMeshAgent.SetDestination(hit.point);
            }
        }
    }

    //Promjena animacije prilikom kretanja
    private void _playerAnimationHandler()
    {
        float currentSpeed = 0;
        if (isAttacking == true)
        {
            currentSpeed = 0;
        }
        else if (isMovingWithKeys == true)
        {
            currentSpeed = direction.magnitude * movementSpeed;
        }
        else
        {
            currentSpeed = _playerNavMeshAgent.velocity.magnitude;
        }
        //1 = run, 0.2 = walk, 0 = idle
        _playerAnimator.SetFloat("RunBlend", currentSpeed / (Stats.Speed.Value * 1.5f), 0.1f, Time.deltaTime);

        //koraci - svakih StepDistance metara jedan korak
        if (currentSpeed > 0.3f)
        {
            stepCounter = stepCounter + currentSpeed * Time.deltaTime;
            if (stepCounter >= StepDistance)
            {
                stepCounter = 0;
                SFX.PlayAt(SFX.Instance.Footsteps, transform.position, 0.6f, 0.15f);
            }
        }
    }
}

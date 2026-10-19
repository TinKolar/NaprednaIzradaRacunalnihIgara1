using UnityEngine;

//Podaci o oruzju. Animacije napada se kod equipa zamjene u animatoru (AnimatorOverrideController).
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game/Weapon Data")]
public class WeaponData : EquippableData
{
    //ime, ikona, Damage i EquipType (OneHanded / TwoHanded) dolaze iz EquippableData i ItemData

    [Header("Visuals")]
    public GameObject Model;          // model koji se pojavi u ruci
    public Vector3 HoldOffset;        // fino podesavanje polozaja u ruci
    public Vector3 HoldRotation;
    public GameObject HeavyAttackVFX; // npr. slash efekt za AOE napad

    [Header("Animations")]
    public AnimationClip LightAttack;
    public AnimationClip HeavyAttack;
    [Tooltip("1 = normalna brzina, 1.5 = 50% brze")]
    public float AttackSpeed = 1f;

    [Header("Light attack (single target)")]
    [Range(0, 1)] public float LightHitTime = 0.4f; // dio animacije kad udarac pogodi
    public float LightRange = 1.5f;
    public float LightRadius = 1.2f;

    [Header("Heavy attack (AOE)")]
    [Range(0, 1)] public float HeavyHitTime = 0.45f;
    public float HeavyRadius = 3.5f;
    public int HeavyDamageMultiplier = 2;
}

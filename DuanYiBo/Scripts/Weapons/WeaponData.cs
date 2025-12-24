using UnityEngine;

[CreateAssetMenu(menuName = "Game/Weapon Data", fileName = "WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponId;
    public string displayName;
    public WeaponType weaponType = WeaponType.Melee;

    [Header("Stats")]
    public int attackPower = 5;
    public float attackRange = 1.5f;
    public float attackCooldown = 0.5f;

    [Header("Animation")]
    public AnimatorOverrideController animatorOverride;

    [Header("UI")]
    public Sprite icon;

    [Header("Audio")]
    public AudioClip attackClip;

    [Header("Ranged")]
    public WeaponProjectile projectilePrefab;
    public float projectileSpeed = 8f;
}

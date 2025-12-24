using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerCombat : MonoBehaviour, IWeaponEquippable
{
    [Header("Stats")]
    [SerializeField] private int baseAttackPower = 10;
    [SerializeField] private float baseAttackRange = 1.2f;
    [SerializeField] private float baseAttackCooldown = 0.6f;

    [Header("Equipment")]
    [SerializeField] private WeaponSlot weaponSlot;

    [Header("Attack")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask attackLayer;

    [Header("Audio")]
    [SerializeField] private AudioSource actionAudioSource;

    private Animator animator;
    private RuntimeAnimatorController baseController;
    private WeaponData currentWeapon;
    private float nextAttackTime;

    public int CurrentAttackPower => baseAttackPower + (currentWeapon != null ? currentWeapon.attackPower : 0);
    public float CurrentAttackRange => currentWeapon != null && currentWeapon.attackRange > 0f ? currentWeapon.attackRange : baseAttackRange;
    public float CurrentAttackCooldown => currentWeapon != null && currentWeapon.attackCooldown > 0f ? currentWeapon.attackCooldown : baseAttackCooldown;
    public WeaponData CurrentWeapon => currentWeapon;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        baseController = animator.runtimeAnimatorController;

        if (actionAudioSource == null)
        {
            TryGetComponent(out actionAudioSource);
        }
    }

    private void OnEnable()
    {
        if (weaponSlot != null)
        {
            weaponSlot.WeaponChanged += HandleWeaponChanged;
            HandleWeaponChanged(weaponSlot.EquippedData);
        }
    }

    private void OnDisable()
    {
        if (weaponSlot != null)
        {
            weaponSlot.WeaponChanged -= HandleWeaponChanged;
        }
    }

    public void Attack()
    {
        if (Time.time < nextAttackTime || animator == null)
        {
            return;
        }

        animator.SetTrigger("Attack");
        nextAttackTime = Time.time + CurrentAttackCooldown;
    }

    public void OnAttackHit()
    {
        if (currentWeapon != null && currentWeapon.weaponType == WeaponType.Ranged)
        {
            PerformRangedAttack();
        }
        else
        {
            PerformMeleeAttack();
        }

        PlayAttackAudio();
    }

    public bool CanEquip(WeaponData data)
    {
        return data != null;
    }

    public void EquipWeapon(WeaponData data)
    {
        if (!CanEquip(data))
        {
            return;
        }

        currentWeapon = data;
        ApplyAnimatorOverride(data);
    }

    public void UnequipWeapon()
    {
        currentWeapon = null;
        ApplyAnimatorOverride(null);
    }

    private void HandleWeaponChanged(WeaponData data)
    {
        if (data == null)
        {
            UnequipWeapon();
        }
        else
        {
            EquipWeapon(data);
        }
    }

    private void ApplyAnimatorOverride(WeaponData data)
    {
        if (animator == null)
        {
            return;
        }

        if (data != null && data.animatorOverride != null)
        {
            animator.runtimeAnimatorController = data.animatorOverride;
        }
        else
        {
            animator.runtimeAnimatorController = baseController;
        }
    }

    private void PerformMeleeAttack()
    {
        Vector2 center = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;
        float range = CurrentAttackRange;

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, range, attackLayer);
        for (int i = 0; i < hits.Length; i++)
        {
            Enemy enemy = hits[i].GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(CurrentAttackPower);
            }
        }
    }

    private void PerformRangedAttack()
    {
        if (currentWeapon == null || currentWeapon.projectilePrefab == null)
        {
            return;
        }

        Vector3 spawnPosition = attackPoint != null ? attackPoint.position : transform.position;
        WeaponProjectile projectile = Instantiate(currentWeapon.projectilePrefab, spawnPosition, Quaternion.identity);
        Vector2 direction = GetFacingDirection();
        projectile.Initialize(direction, currentWeapon.projectileSpeed, CurrentAttackPower, gameObject, CurrentAttackRange);
    }

    private Vector2 GetFacingDirection()
    {
        float x = transform.localScale.x;
        return x >= 0f ? Vector2.right : Vector2.left;
    }

    private void PlayAttackAudio()
    {
        if (actionAudioSource == null || currentWeapon == null || currentWeapon.attackClip == null)
        {
            return;
        }

        actionAudioSource.PlayOneShot(currentWeapon.attackClip);
    }
}

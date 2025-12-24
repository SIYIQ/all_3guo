using UnityEngine;

public class RangedEnemy : Enemy
{
    [SerializeField] private Arrow projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int attackDamage = 8;
    [SerializeField] private float projectileSpeed = 8f;

    protected override void ExecuteAttack()
    {
        if (projectilePrefab == null)
        {
            return;
        }

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        Arrow projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        Vector2 direction = IsFacingRight ? Vector2.right : Vector2.left;
        projectile.Initialize(direction, projectileSpeed, attackDamage, gameObject);
    }
}

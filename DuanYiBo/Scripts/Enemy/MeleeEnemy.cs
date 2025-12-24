using UnityEngine;

public class MeleeEnemy : Enemy
{
    [SerializeField] private int attackDamage = 10;

    protected override void ExecuteAttack()
    {
        Transform target = CurrentTarget;
        if (target == null || !IsTargetInAttackRange(target))
        {
            return;
        }

        target.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
    }
}

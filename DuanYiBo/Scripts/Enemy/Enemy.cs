using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D), typeof(Animator))]
public abstract class Enemy : MonoBehaviour
{
    private enum EnemyState
    {
        Idle = 0,
        Patrol = 1,
        Chase = 2,
        Attack = 3,
        Hurt = 4,
        Dead = 5,
    }

    [Header("Stats")]
    [SerializeField] private int maxHealth = 30;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float patrolRadius = 3f;
    [SerializeField] private float patrolWaitTime = 1f;
    [SerializeField] private float patrolArriveDistance = 0.2f;

    [Header("Detection")]
    [SerializeField] private Transform detectZone;
    [SerializeField] private Transform attackZone;
    [SerializeField] private float fallbackDetectRadius = 3f;
    [SerializeField] private float fallbackAttackRadius = 1f;
    [SerializeField] private LayerMask targetLayer = ~0;

    [Header("Combat")]
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private float hurtDuration = 0.35f;

    [Header("Audio")]
    [SerializeField] private AudioSource actionAudioSource;
    [SerializeField] private AudioSource moveLoopSource;
    [SerializeField] private AudioClip moveLoopClip;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip hurtClip;
    [SerializeField] private AudioClip deathClip;

    private Rigidbody2D rb;
    private Animator animator;

    private int currentHealth;
    private EnemyState state = EnemyState.Idle;
    private EnemyState lastState = EnemyState.Idle;

    private Transform target;
    private Vector2 lastKnownPosition;
    private Vector2 patrolCenter;
    private Vector2 patrolTarget;
    private float patrolWaitTimer;

    private float attackTimer;
    private float hurtTimer;

    private float desiredVelocityX;
    private bool isFacingRight = true;

    private CircleCollider2D detectZoneCollider;
    private CircleCollider2D attackZoneCollider;

    protected Transform CurrentTarget => target;
    protected bool IsFacingRight => isFacingRight;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;

        if (actionAudioSource == null)
        {
            TryGetComponent(out actionAudioSource);
        }

        if (detectZone != null)
        {
            detectZoneCollider = detectZone.GetComponent<CircleCollider2D>();
        }

        if (attackZone != null)
        {
            attackZoneCollider = attackZone.GetComponent<CircleCollider2D>();
        }

        patrolCenter = transform.position;
        PickNextPatrolPoint();
    }

    protected virtual void Update()
    {
        if (state == EnemyState.Dead)
        {
            UpdateAnimator();
            return;
        }

        UpdateTargetTracking();

        if (state == EnemyState.Hurt)
        {
            hurtTimer -= Time.deltaTime;
            if (hurtTimer <= 0f)
            {
                SetState(EnemyState.Idle);
            }

            desiredVelocityX = 0f;
            UpdateAnimator();
            UpdateMoveLoopAudio();
            return;
        }

        if (target != null)
        {
            lastKnownPosition = target.position;
            if (IsTargetInAttackRange(target))
            {
                HandleAttack();
            }
            else
            {
                HandleChase();
            }
        }
        else
        {
            HandlePatrol();
        }

        UpdateAnimator();
        UpdateMoveLoopAudio();
    }

    protected virtual void FixedUpdate()
    {
        if (state == EnemyState.Dead)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (state == EnemyState.Hurt)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        rb.velocity = new Vector2(desiredVelocityX, rb.velocity.y);
    }

    public void TakeDamage(int amount)
    {
        if (state == EnemyState.Dead || amount <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        hurtTimer = hurtDuration;
        SetState(EnemyState.Hurt);
        PlayOneShot(hurtClip);
    }

    protected abstract void ExecuteAttack();

    protected bool IsTargetInAttackRange(Transform candidate)
    {
        if (candidate == null)
        {
            return false;
        }

        Vector2 center = GetZoneCenter(attackZone, attackZoneCollider);
        float radius = GetZoneRadius(attackZone, attackZoneCollider, fallbackAttackRadius);
        float sqrDistance = ((Vector2)candidate.position - center).sqrMagnitude;
        return sqrDistance <= radius * radius;
    }

    protected void PlayAttackClip()
    {
        PlayOneShot(attackClip);
    }

    private void HandleAttack()
    {
        desiredVelocityX = 0f;
        SetState(EnemyState.Attack);

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            ExecuteAttack();
            PlayAttackClip();
            attackTimer = attackCooldown;
        }
    }

    private void HandleChase()
    {
        SetState(EnemyState.Chase);
        desiredVelocityX = GetMoveVelocityX(target.position, chaseSpeed);
    }

    private void HandlePatrol()
    {
        if (patrolWaitTimer > 0f)
        {
            patrolWaitTimer -= Time.deltaTime;
            desiredVelocityX = 0f;
            SetState(EnemyState.Idle);
            return;
        }

        float distance = Mathf.Abs(patrolTarget.x - transform.position.x);
        if (distance <= patrolArriveDistance)
        {
            patrolWaitTimer = patrolWaitTime;
            desiredVelocityX = 0f;
            SetState(EnemyState.Idle);
            PickNextPatrolPoint();
            return;
        }

        SetState(EnemyState.Patrol);
        desiredVelocityX = GetMoveVelocityX(patrolTarget, moveSpeed);
    }

    private float GetMoveVelocityX(Vector2 destination, float speed)
    {
        float deltaX = destination.x - transform.position.x;
        if (Mathf.Abs(deltaX) < 0.01f)
        {
            return 0f;
        }

        float direction = Mathf.Sign(deltaX);
        UpdateFacing(direction);
        return direction * speed;
    }

    private void UpdateFacing(float direction)
    {
        if (direction > 0.01f && !isFacingRight)
        {
            Flip();
        }
        else if (direction < -0.01f && isFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private void UpdateTargetTracking()
    {
        if (target != null)
        {
            if (!IsTargetInDetectRange(target))
            {
                lastKnownPosition = target.position;
                target = null;
                patrolCenter = lastKnownPosition;
                patrolWaitTimer = 0f;
                PickNextPatrolPoint();
            }

            return;
        }

        target = FindTargetInDetectRange();
        if (target != null)
        {
            lastKnownPosition = target.position;
        }
    }

    private Transform FindTargetInDetectRange()
    {
        Vector2 center = GetZoneCenter(detectZone, detectZoneCollider);
        float radius = GetZoneRadius(detectZone, detectZoneCollider, fallbackDetectRadius);
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, targetLayer);
        if (hits == null || hits.Length == 0)
        {
            return null;
        }

        Transform bestTarget = null;
        float bestDistance = float.PositiveInfinity;
        foreach (Collider2D hit in hits)
        {
            if (hit == null || !hit.CompareTag("Player"))
            {
                continue;
            }

            float sqrDistance = ((Vector2)hit.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (sqrDistance < bestDistance)
            {
                bestDistance = sqrDistance;
                bestTarget = hit.transform;
            }
        }

        return bestTarget;
    }

    private bool IsTargetInDetectRange(Transform candidate)
    {
        if (candidate == null)
        {
            return false;
        }

        Vector2 center = GetZoneCenter(detectZone, detectZoneCollider);
        float radius = GetZoneRadius(detectZone, detectZoneCollider, fallbackDetectRadius);
        float sqrDistance = ((Vector2)candidate.position - center).sqrMagnitude;
        return sqrDistance <= radius * radius;
    }

    private Vector2 GetZoneCenter(Transform zone, CircleCollider2D zoneCollider)
    {
        if (zoneCollider != null)
        {
            return zoneCollider.bounds.center;
        }

        if (zone != null)
        {
            return zone.position;
        }

        return transform.position;
    }

    private float GetZoneRadius(Transform zone, CircleCollider2D zoneCollider, float fallbackRadius)
    {
        if (zoneCollider != null)
        {
            float scale = 1f;
            if (zone != null)
            {
                scale = Mathf.Max(zone.lossyScale.x, zone.lossyScale.y);
            }

            return Mathf.Abs(zoneCollider.radius * scale);
        }

        return fallbackRadius;
    }

    private void PickNextPatrolPoint()
    {
        Vector2 offset = Random.insideUnitCircle * patrolRadius;
        patrolTarget = new Vector2(patrolCenter.x + offset.x, patrolCenter.y);
    }

    private void Die()
    {
        SetState(EnemyState.Dead);
        rb.velocity = Vector2.zero;
        PlayOneShot(deathClip);
        StopMoveLoopAudio();
    }

    private void SetState(EnemyState newState)
    {
        state = newState;
    }

    private void UpdateAnimator()
    {
        if (animator == null)
        {
            return;
        }

        if (state != lastState)
        {
            animator.SetInteger("State", (int)state);
            lastState = state;
        }

        animator.SetFloat("Speed", Mathf.Abs(desiredVelocityX));
    }

    private void UpdateMoveLoopAudio()
    {
        if (moveLoopSource == null || moveLoopClip == null)
        {
            return;
        }

        bool shouldPlay = (state == EnemyState.Patrol || state == EnemyState.Chase) && Mathf.Abs(desiredVelocityX) > 0.01f;
        if (shouldPlay)
        {
            if (!moveLoopSource.isPlaying)
            {
                moveLoopSource.clip = moveLoopClip;
                moveLoopSource.loop = true;
                moveLoopSource.Play();
            }
        }
        else
        {
            StopMoveLoopAudio();
        }
    }

    private void StopMoveLoopAudio()
    {
        if (moveLoopSource != null && moveLoopSource.isPlaying)
        {
            moveLoopSource.Stop();
        }
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (actionAudioSource == null || clip == null)
        {
            return;
        }

        actionAudioSource.PlayOneShot(clip);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D), typeof(Animator))]
public class CharacterController : MonoBehaviour
{
    private enum CharacterState
    {
        Idle = 0,
        Move = 1,
        Jump = 2,
        Fall = 3,
        Hurt = 4,
        Dead = 5,
        DoubleJump = 6,
    }

    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int maxMana = 50;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float groundCheckDistance = 0.05f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private int extraJumps = 1;

    [Header("Hurt")]
    [SerializeField] private float hurtDuration = 0.35f;

    [Header("Audio")]
    [SerializeField] private AudioSource actionAudioSource;
    [SerializeField] private AudioSource moveLoopSource;
    [SerializeField] private AudioClip moveLoopClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip hurtClip;
    [SerializeField] private AudioClip deathClip;

    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private Animator animator;

    private int currentHealth;
    private int currentMana;

    private float horizontalInput;
    private bool jumpPressed;
    private bool isGrounded;
    private bool isFacingRight = true;
    private int jumpsRemaining;

    private float hurtTimer;
    private CharacterState state = CharacterState.Idle;
    private CharacterState lastState = CharacterState.Idle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        currentMana = maxMana;
        jumpsRemaining = Mathf.Max(0, extraJumps);

        if (actionAudioSource == null)
        {
            TryGetComponent(out actionAudioSource);
        }
    }

    private void Update()
    {
        if (state == CharacterState.Dead)
        {
            UpdateAnimator();
            return;
        }

        horizontalInput = Input.GetAxisRaw("Horizontal");
        jumpPressed = Input.GetKeyDown(KeyCode.Space);
        isGrounded = CheckGrounded();
        if (isGrounded)
        {
            jumpsRemaining = Mathf.Max(0, extraJumps);
        }

        if (state == CharacterState.Hurt)
        {
            hurtTimer -= Time.deltaTime;
            if (hurtTimer <= 0f)
            {
                SetState(EvaluateLocomotionState());
            }

            UpdateAnimator();
            UpdateMoveLoopAudio();
            return;
        }

        bool jumpedThisFrame = false;
        if (jumpPressed)
        {
            if (isGrounded)
            {
                Jump();
                jumpedThisFrame = true;
            }
            else if (jumpsRemaining > 0)
            {
                DoubleJump();
                jumpsRemaining--;
                jumpedThisFrame = true;
            }
        }

        if (!jumpedThisFrame)
        {
            SetState(EvaluateLocomotionState());
        }

        HandleFlip();
        UpdateAnimator();
        UpdateMoveLoopAudio();
    }

    private void FixedUpdate()
    {
        if (state == CharacterState.Dead || state == CharacterState.Hurt)
        {
            return;
        }

        float targetX = horizontalInput * moveSpeed;
        rb.velocity = new Vector2(targetX, rb.velocity.y);
    }

    public void TakeDamage(int amount)
    {
        if (state == CharacterState.Dead || amount <= 0)
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
        SetState(CharacterState.Hurt);
        PlayOneShot(hurtClip);
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || state == CharacterState.Dead)
        {
            return;
        }

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    public bool UseMana(int amount)
    {
        if (amount <= 0 || state == CharacterState.Dead)
        {
            return false;
        }

        if (currentMana < amount)
        {
            return false;
        }

        currentMana -= amount;
        return true;
    }

    public void RestoreMana(int amount)
    {
        if (amount <= 0 || state == CharacterState.Dead)
        {
            return;
        }

        currentMana = Mathf.Min(maxMana, currentMana + amount);
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        SetState(CharacterState.Jump);
        PlayOneShot(jumpClip);
    }

    private void DoubleJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        SetState(CharacterState.DoubleJump);
        PlayOneShot(jumpClip);
    }

    private void Die()
    {
        SetState(CharacterState.Dead);
        rb.velocity = Vector2.zero;
        PlayOneShot(deathClip);
        StopMoveLoopAudio();
    }

    private CharacterState EvaluateLocomotionState()
    {
        if (!isGrounded)
        {
            if (state == CharacterState.DoubleJump && rb.velocity.y >= 0.01f)
            {
                return CharacterState.DoubleJump;
            }

            return rb.velocity.y >= 0.01f ? CharacterState.Jump : CharacterState.Fall;
        }

        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            return CharacterState.Move;
        }

        return CharacterState.Idle;
    }

    private void SetState(CharacterState newState)
    {
        state = newState;
    }

    private void HandleFlip()
    {
        if (horizontalInput > 0.01f && !isFacingRight)
        {
            Flip();
        }
        else if (horizontalInput < -0.01f && isFacingRight)
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

    private bool CheckGrounded()
    {
        if (capsuleCollider == null)
        {
            return false;
        }

        Bounds bounds = capsuleCollider.bounds;
        Vector2 boxSize = new Vector2(bounds.size.x * 0.9f, bounds.size.y);
        RaycastHit2D hit = Physics2D.BoxCast(bounds.center, boxSize, 0f, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider != null;
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

        animator.SetBool("Grounded", isGrounded);
        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
        animator.SetFloat("VerticalSpeed", rb.velocity.y);
    }

    private void UpdateMoveLoopAudio()
    {
        if (moveLoopSource == null || moveLoopClip == null)
        {
            return;
        }

        bool shouldPlay = state == CharacterState.Move && isGrounded && Mathf.Abs(horizontalInput) > 0.01f;
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

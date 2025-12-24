using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class WeaponProjectile : MonoBehaviour
{
    [SerializeField] private float maxLifetime = 3f;

    private Rigidbody2D rb;
    private Collider2D hitCollider;
    private int damage;
    private Vector2 direction = Vector2.right;
    private float speed;
    private GameObject owner;
    private float maxDistance;
    private Vector2 startPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hitCollider = GetComponent<Collider2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        hitCollider.isTrigger = true;
    }

    public void Initialize(Vector2 direction, float speed, int damage, GameObject owner, float maxDistance)
    {
        this.direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
        this.speed = Mathf.Max(0f, speed);
        this.damage = damage;
        this.owner = owner;
        this.maxDistance = Mathf.Max(0f, maxDistance);
        startPosition = rb.position;

        transform.right = this.direction;
        rb.velocity = this.direction * this.speed;
        Destroy(gameObject, maxLifetime);
    }

    private void Update()
    {
        if (maxDistance <= 0f)
        {
            return;
        }

        float traveled = Vector2.Distance(startPosition, rb.position);
        if (traveled >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null || other.isTrigger)
        {
            return;
        }

        if (owner != null && (other.gameObject == owner || other.transform.IsChildOf(owner.transform)))
        {
            return;
        }

        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}

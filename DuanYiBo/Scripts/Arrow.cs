using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Arrow : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;

    private Rigidbody2D rb;
    private Collider2D hitCollider;
    private int damage;
    private Vector2 direction = Vector2.right;
    private float speed;
    private GameObject owner;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hitCollider = GetComponent<Collider2D>();
        rb.gravityScale = 0f;
        rb.isKinematic = true;
        hitCollider.isTrigger = true;
    }

    public void Initialize(Vector2 direction, float speed, int damage, GameObject owner)
    {
        this.direction = direction.normalized;
        this.speed = speed;
        this.damage = damage;
        this.owner = owner;

        transform.right = this.direction;
        rb.velocity = this.direction * this.speed;
        Destroy(gameObject, lifetime);
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

        if (other.CompareTag("Player"))
        {
            other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }

        Destroy(gameObject);
    }
}

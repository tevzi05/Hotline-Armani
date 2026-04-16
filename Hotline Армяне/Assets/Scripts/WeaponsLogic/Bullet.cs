using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 50f;
    public int damage = 25;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Устанавливаем скорость через физику
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, 2f);
    }

    // Update больше не нужен для движения!

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            Zombie zombie = other.GetComponent<Zombie>();
            if (zombie != null)
            {
                zombie.TakeDamage(damage);
                Destroy(gameObject);
            }
            else if (other.CompareTag("Wall"))
            {
                Destroy(gameObject);
            }
            else if (!other.CompareTag("Player"))
            {
                Destroy(gameObject);
            }
        }
    }
}
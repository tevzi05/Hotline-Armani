using UnityEngine;

public class Zombie : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;

    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Attack")]
    public int damageToPlayer = 20;      // урон игроку
    public float attackCooldown = 1f;    // задержка между ударами
    private float lastAttackTime;

    private Transform player;

    private void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        transform.Translate(direction * speed * Time.deltaTime);
    }

    // тригер удара
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && Time.time > lastAttackTime + attackCooldown)
        {
            Player playerHealth = other.GetComponent<Player>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageToPlayer);
                lastAttackTime = Time.time;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}
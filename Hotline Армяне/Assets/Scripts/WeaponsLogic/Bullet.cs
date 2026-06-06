using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 50f;
    public int damage = 25;

    // Этот флаг будет настраивать пушка при выстреле.
    // Если true — стрелял зомби. Если false — стрелял игрок.
    [HideInInspector] public bool isEnemyBullet = false;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, 2f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. ЕСЛИ ПУЛЯ ВРЕЗАЛАСЬ В ИГРОКА
        if (other.CompareTag("Player"))
        {
            // Если это пуля игрока (рикошет или баг коллизии) — игнорируем
            if (!isEnemyBullet) return;

            // Если это пуля ЗОМБИ — наносим урон игроку и уничтожаем пулю
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            Destroy(gameObject);
            return;
        }

        // 2. ЕСЛИ ПУЛЯ ВРЕЗАЛАСЬ В ПРОТИВНИКА
        Zombie enemy = other.GetComponent<Zombie>();

        if (enemy != null)
        {
            
            if (isEnemyBullet) return;

            // Если стрелял ИГРОК — наносим урон зомби
            if (enemy != null) enemy.TakeDamage(damage);
           
            Destroy(gameObject);
            return;
        }

        // 3. ЕСЛИ ПУЛЯ ВРЕЗАЛАСЬ В СТЕНУ ИЛИ ДРУГОЙ ОБЪЕКТ
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Player"))
        {
            // На всякий случай уничтожаем при любых других коллизиях
            Destroy(gameObject);
        }
    }
}

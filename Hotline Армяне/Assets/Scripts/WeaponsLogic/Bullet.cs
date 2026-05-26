using UnityEngine;
using Unity.Netcode;  // Добавь, если нет

//public class Bullet : MonoBehaviour
public class Bullet : NetworkBehaviour
{
    public float speed = 50f;
    public int damage = 25;
    public ulong ownerNetId; // ID игрока, который выстрелил
    private Rigidbody2D rb;
    public Team shooterTeam;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Устанавливаем скорость через физику
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, 2f);
    }

    // Update больше не нужен для движения!

    //void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (!other.CompareTag("Player"))
    //    {
    //        Zombie zombie = other.GetComponent<Zombie>();
    //        if (zombie != null)
    //        {
    //            zombie.TakeDamage(damage);
    //            Destroy(gameObject);
    //        }
    //        else if (other.CompareTag("Wall"))
    //        {
    //            Destroy(gameObject);
    //        }
    //        else if (!other.CompareTag("Player"))
    //        {
    //            Destroy(gameObject);
    //        }
    //    }
    //}
    void OnTriggerEnter2D(Collider2D other)
    {
        // Попадание по игроку (PvP)
        NetworkPlayer targetPlayer = other.GetComponent<NetworkPlayer>();
        if (targetPlayer != null)
        {
            // Нельзя убить себя
            if (targetPlayer.OwnerClientId == ownerNetId) return;

            if (targetPlayer.GetTeam() == shooterTeam) return;

            // Наносим урон только на сервере
            if (IsServer)
            {
                targetPlayer.TakeDamage(damage, ownerNetId);
            }
            Destroy(gameObject);
            return;
        }

        // Остальные коллизии (зомби, стены, и т.д.)
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
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 40f;
    public int damage = 25; // добавили поле для урона

    void Start()
    {
        Destroy(gameObject, 2f); // Самоуничтожение через 2 секунды

    }

    void Update()
    {
        // Движение вперед
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            // Пытаемся получить компонент Zombie на объекте, в который попали
            Zombie zombie = other.GetComponent<Zombie>();
            if (zombie != null)
            {
                zombie.TakeDamage(damage); // наносим урон зомби
                Destroy(gameObject);       // пуля исчезает
            }
            // Если это не зомби и не игрок (например, стена) – тоже уничтожаем пулю
            else if (!other.CompareTag("Player"))
            {
                Destroy(gameObject);
            }
            //Destroy(gameObject); // Уничтожить при попадании
        }
    }
}
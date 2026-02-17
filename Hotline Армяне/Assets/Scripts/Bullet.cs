using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 40f;

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
            Destroy(gameObject); // Уничтожить при попадании
        }
    }
}
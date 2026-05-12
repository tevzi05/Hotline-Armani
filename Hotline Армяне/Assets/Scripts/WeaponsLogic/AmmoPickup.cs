using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [Header("Settings")]
    //[SerializeField] private int ammoAmount = 10; // сколько патронов даёт один pickup
    [SerializeField] private AudioClip pickupSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                // Добавляем патроны текущему оружию
                player.AddAmmo();

                // Звук (опционально)
                AudioSource source = other.GetComponent<AudioSource>();
                if (source != null && pickupSound != null)
                    source.PlayOneShot(pickupSound);

                Destroy(gameObject);
            }
        }
    }
}
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private AudioClip pickupSound;
    private bool canPickup = false; // Флаг: находится ли игрок в зоне
    private Player playerInZone;    // Ссылка на игрока

    private void Update()
    {
        // Если игрок в зоне И нажал E
        if (canPickup && Input.GetKeyDown(KeyCode.E))
        {
            PickUp();
        }
    }

    private void PickUp()
    {
        // Воспроизводим звук через AudioSource игрока
        AudioSource playerSource = playerInZone.GetComponent<AudioSource>();
        if (playerSource != null && pickupSound != null)
        {
            playerSource.PlayOneShot(pickupSound);
        }

        // Передаем данные оружия игроку
        playerInZone.EquipWeapon(weaponData);

        // Удаляем предмет
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = other.GetComponent<Player>();
            if (playerInZone != null)
            {
                canPickup = true;
                // Здесь можно включить UI подсказку "Нажми E"
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canPickup = false;
            playerInZone = null;
            // Здесь можно скрыть подсказку
        }
    }
}

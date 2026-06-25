using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private AudioClip pickupSound;
    private bool canPickup = false;

    private Player singlePlayerInZone;
   

    private void Update()
    {
        if (canPickup && Input.GetKeyDown(KeyCode.Mouse1))
        {
            PickUp();
        }
    }

    private void PickUp()
    {
        // ВАРИАНТ 1: Если мы в одиночной игре
        if (singlePlayerInZone != null)
        {
            AudioSource playerSource = singlePlayerInZone.GetComponent<AudioSource>();
            if (playerSource != null && pickupSound != null)
            {
                playerSource.PlayOneShot(pickupSound);
            }

            singlePlayerInZone.EquipWeapon(weaponData);
            Destroy(gameObject); // В сингле обычный Destroy работает отлично
            return;
        }

    
    }

    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем синглплеер
        singlePlayerInZone = other.GetComponent<Player>();
        if (singlePlayerInZone != null)
        {
            canPickup = true;
            return;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Сбрасываем синглплеер
        if (singlePlayerInZone != null && other.gameObject == singlePlayerInZone.gameObject)
        {
            canPickup = false;
            singlePlayerInZone = null;
        }
    }
}

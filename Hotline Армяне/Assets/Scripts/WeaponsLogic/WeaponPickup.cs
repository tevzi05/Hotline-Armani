using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private WeaponData weaponData; // Перетащи сюда файл WeaponData из папки Project
    [SerializeField] private AudioClip pickupSound;
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, что это игрок
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();

            // Если игрок найден и у него еще нет оружия (или хочешь заменить текущее)
            if (player != null) //&& !player.HasWeapon())
            {
                AudioSource playerSource = other.GetComponent<AudioSource>();
                if (playerSource != null && pickupSound != null)
                {
                    playerSource.PlayOneShot(pickupSound);
                }
                // Передаем данные конкретного оружия игроку
                player.EquipWeapon(weaponData);

                // Удаляем предмет с земли
                Destroy(gameObject);
            }
        }
    }
}

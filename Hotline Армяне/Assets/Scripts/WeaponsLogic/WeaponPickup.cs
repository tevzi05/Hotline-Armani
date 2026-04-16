using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private WeaponData weaponData; // Перетащи сюда файл WeaponData из папки Project

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, что это игрок
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();

            // Если игрок найден и у него еще нет оружия (или хочешь заменить текущее)
            if (player != null && !player.HasWeapon())
            {
                // Передаем данные конкретного оружия игроку
                player.EquipWeapon(weaponData);

                // Удаляем предмет с земли
                Destroy(gameObject);
            }
        }
    }
}

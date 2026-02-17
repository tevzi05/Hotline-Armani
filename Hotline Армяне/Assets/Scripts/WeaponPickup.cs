using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null && !player.HasWeapon())
            {
                player.EquipWeapon();
                Destroy(gameObject); // Убрать оружие с земли
            }
        }
    }
}
using UnityEngine;
using Unity.Netcode; // Обязательно добавляем Netcode

public class WeaponPickup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private AudioClip pickupSound;
    private bool canPickup = false;

    // Ссылки на оба типа игроков (одна из них будет null в зависимости от режима)
    private Player singlePlayerInZone;
    private NetworkPlayer networkPlayerInZone;

    private void Update()
    {
        if (canPickup && Input.GetKeyDown(KeyCode.E))
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

        // ВАРИАНТ 2: Если мы в сетевой игре
        if (networkPlayerInZone != null)
        {
            // Нажать кнопку 'E' и инициировать подбор может ТОЛЬКО владелец этого персонажа
            if (networkPlayerInZone.IsOwner)
            {
                AudioSource playerSource = networkPlayerInZone.GetComponent<AudioSource>();
                if (playerSource != null && pickupSound != null)
                {
                    playerSource.PlayOneShot(pickupSound);
                }

                networkPlayerInZone.EquipWeapon(weaponData);

                // Запрашиваем у сервера удаление оружия из сети для ВСЕХ игроков
                RequestDespawnServerRpc();
            }
        }
    }

    // Серверный метод, который удаляет объект оружия из игрового мира у всех клиентов
    [ServerRpc(RequireOwnership = false)] // RequireOwnership = false позволяет клиенту вызывать этот метод, даже если он не владеет этим оружием на земле
    private void RequestDespawnServerRpc()
    {
        NetworkObject no = GetComponent<NetworkObject>();
        if (no != null && no.IsSpawned)
        {
            no.Despawn(); // Сетевое удаление объекта сервером
        }
        else
        {
            // На случай, если вы еще не добавили NetworkObject на оружие, 
            // сервер просто удалит его физически, но лучше добавить NetworkObject.
            Destroy(gameObject);
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

        // Проверяем мультиплеер
        networkPlayerInZone = other.GetComponent<NetworkPlayer>();
        if (networkPlayerInZone != null)
        {
            canPickup = true;
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

        // Сбрасываем мультиплеер
        if (networkPlayerInZone != null && other.gameObject == networkPlayerInZone.gameObject)
        {
            canPickup = false;
            networkPlayerInZone = null;
        }
    }
}

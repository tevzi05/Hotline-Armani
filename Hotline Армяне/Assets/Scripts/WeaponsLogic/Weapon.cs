using UnityEngine;
using UnityEngine.Audio;

public class Weapon : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource; // Ссылка на AudioSource
    [SerializeField] private AudioMixerGroup sfxGroup; // Ссылка на группу микшера SFX

    private WeaponData currentWeaponData;
    private int currentAmmo;
    private int ammoReserve;
    private int magazineSize;

    // Метод Init теперь принимает ТОЛЬКО данные оружия
    public void Init(WeaponData data)
    {
        currentWeaponData = data;
        currentAmmo = data.maxAmmo;
        magazineSize = data.maxAmmo;
        ammoReserve = data.maxReserveAmmo;

        // Принудительно связываем их при экипировке оружия
        if (audioSource != null && sfxGroup != null)
        {
            audioSource.outputAudioMixerGroup = sfxGroup;
        }

        // Звук поднятия оружия
        PlaySound(currentWeaponData.weaponPickup, currentWeaponData.weaponVolume, 1f, false);
    }

    public bool CanShoot(float nextFireTime) => currentWeaponData != null && Time.time >= nextFireTime;


    public float Fire(Transform firePoint)
    {
        if (currentAmmo > 0)
        {
            // Спавним пулю
            GameObject bulletObj = Instantiate(currentWeaponData.bulletPrefab, firePoint.position, firePoint.rotation);

            // --- БЛОК НАСТРОЙКИ ПУЛИ: ---
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                // Проверяем: если этот скрипт Weapon висит НЕ на игроке (а на зомби),
                // то помечаем пулю как вражескую
                if (GetComponentInParent<Player>() == null)
                {
                    bulletScript.isEnemyBullet = true;
                }
            }
            // -----------------------------

            float randomPitch = Random.Range(currentWeaponData.minPitch, currentWeaponData.maxPitch);
            PlaySound(currentWeaponData.shootSound, currentWeaponData.shootVolume, randomPitch, true);
            currentAmmo--;
            return currentWeaponData.fireRate;
        }
        else
        {
            PlaySound(currentWeaponData.emptySound, currentWeaponData.emptyVolume, 1f, false);
            return 0.25f;
        }
    }


    public void AddAmmo()
    {
        if (currentWeaponData != null) ammoReserve += currentWeaponData.ammoPerKill;
    }

    public bool NeedsReload() => currentAmmo < magazineSize && ammoReserve > 0;

    public void ExecuteReload()
    {
        int amountNeeded = magazineSize - currentAmmo;
        int amountToTake = Mathf.Min(amountNeeded, ammoReserve);
        currentAmmo += amountToTake;
        ammoReserve -= amountToTake;

        // Если добавил звук перезарядки в WeaponData:
        // PlaySound(currentWeaponData.reloadSound, currentWeaponData.reloadVolume, 1f, false);
    }

    private void PlaySound(AudioClip clip, float volume, float pitch, bool allowOverlap)
    {
        if (clip == null || audioSource == null) return;

        audioSource.pitch = pitch;
        if (allowOverlap)
        {
            audioSource.PlayOneShot(clip, volume);
        }
        else if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    public string GetAmmoText() => $"{currentAmmo}/{ammoReserve}";
    public bool IsOutofAmmo() => ammoReserve == 0 && currentAmmo == 0;
    public bool IsMagazineEmpty() => currentAmmo == 0;
}

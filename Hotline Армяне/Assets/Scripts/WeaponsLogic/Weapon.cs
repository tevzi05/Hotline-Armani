using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using Unity.VisualScripting;

public class Weapon : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource; // Ссылка на AudioSource
    [SerializeField] private AudioMixerGroup sfxGroup; // Ссылка на группу микшера SFX

    private WeaponData currentWeaponData;
    private int currentAmmo;
    private int ammoReserve;
    private int magazineSize;
    private bool isReloading = false;
    private bool interruptReload = false;
    public bool IsReloading => isReloading;

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

    public bool CanShoot(float nextFireTime) => currentWeaponData != null && Time.time >= nextFireTime && !isReloading;


    public float Fire(Transform firePoint)
    {
        if (isReloading) return 0f;

        if (currentAmmo > 0)
        {
            // ПРОВЕРЯЕМ: ДРОБОВИК ИЛИ ОБЫЧНАЯ ПУШКА?
            if (currentWeaponData.isShotgun)
            {
                // Цикл выпускает столько дробинок, сколько указано в WeaponData
                for (int i = 0; i < currentWeaponData.pelletsCount; i++)
                {
                    // Считаем случайное отклонение угла для каждой дробинки
                    float randomSpread = Random.Range(-currentWeaponData.spreadAngle / 2f, currentWeaponData.spreadAngle / 2f);
                    Quaternion pelletRotation = firePoint.rotation * Quaternion.Euler(0, 0, randomSpread);
                    GameObject pelletObj = Instantiate(currentWeaponData.bulletPrefab, firePoint.position, pelletRotation);
                    SetupBulletProperties(pelletObj);
                }
            }
            else
            {
                // Логика любой пушки кроме дробовика
                GameObject bulletObj = Instantiate(currentWeaponData.bulletPrefab, firePoint.position, firePoint.rotation);
                SetupBulletProperties(bulletObj);
            }

            // Звук выстрела (остается единым)
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

    private void SetupBulletProperties(GameObject bulletObj)
    {
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            if (GetComponentInParent<Player>() == null)
            {
                bulletScript.isEnemyBullet = true;
            }
        }
    }


    public void AddAmmo()
    {
        if (currentWeaponData != null) ammoReserve += currentWeaponData.ammoPerKill;
    }

    public bool NeedsReload() => currentAmmo < magazineSize && ammoReserve > 0 && !isReloading;

    public void StartReload(System.Action onReloadComplete)
    {
        if (!NeedsReload()) return;
        interruptReload = false;
        if (currentWeaponData.isShotgun)
        {
            StartCoroutine(ShotgunReloadCoroutine(onReloadComplete));
        }
        else
        {
            StartCoroutine(ReloadCoroutine(onReloadComplete));
        }


    }

    private IEnumerator ShotgunReloadCoroutine(System.Action onReloadComplete)
    {
        isReloading = true;
        PlaySound(currentWeaponData.cocking, currentWeaponData.cockingVolume, 1f, true);

        yield return new WaitForSeconds(0.3f);

        while (currentAmmo < magazineSize && ammoReserve > 0 && !interruptReload)
        {

            yield return new WaitForSeconds(currentWeaponData.reloadTime);

            if (interruptReload) break;

            currentAmmo++;
            ammoReserve--;
            onReloadComplete?.Invoke();

            PlaySound(currentWeaponData.weaponReloadSound, currentWeaponData.reloadVolume, 1f, true);
        }
        yield return new WaitForSeconds(0.3f);

        PlaySound(currentWeaponData.weaponPickup, currentWeaponData.weaponVolume, 1f, true);
        isReloading = false;
        onReloadComplete?.Invoke();

    }

    public void TryInterruptReload()
    {
        if (currentWeaponData != null && currentWeaponData.isShotgun && isReloading)
        {
            interruptReload = true;
            Debug.Log("ща стрельну");
        }
    }

    private IEnumerator ReloadCoroutine(System.Action onReloadComplete)
    {
        isReloading = true;


        PlaySound(currentWeaponData.weaponReloadSound, currentWeaponData.reloadVolume, 1f, true);
        yield return new WaitForSeconds(currentWeaponData.reloadTime);

        // Логика пересчета патронов (бывший ExecuteReload)
        int amountNeeded = magazineSize - currentAmmo;
        int amountToTake = Mathf.Min(amountNeeded, ammoReserve);
        currentAmmo += amountToTake;
        ammoReserve -= amountToTake;

        isReloading = false;

        onReloadComplete?.Invoke();
    }

    public void ForceInstantReload()
    {
        if (currentWeaponData == null) return;

        int amountNeeded = magazineSize - currentAmmo;
        int amountToTake = Mathf.Min(amountNeeded, ammoReserve);
        currentAmmo += amountToTake;
        ammoReserve -= amountToTake;
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
    public bool IsMagazineLowOnAmmo() => currentAmmo <= magazineSize / 3;
    public bool IsMagazineEmpty() => currentAmmo == 0;
}

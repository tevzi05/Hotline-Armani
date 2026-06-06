using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public GameObject bulletPrefab;
    public float fireRate = 0.15f;
    public int maxAmmo = 30;
    public int maxReserveAmmo = 30;

    // логика дл€ дробовика
    [Header("Shotgun & Spread Settings")]
    public bool isShotgun = false;       
    public int pelletsCount = 5;         // —колько дробинок вылетает за один выстрел
    public float spreadAngle = 15f;      // ”гол разлета дроби в градусах

    [Header("Economy")]
    public int ammoPerKill = 5;

    // —сылка на уникальные анимации дл€ этой пушки
    [Header("Animations")]
    public RuntimeAnimatorController weaponOverride;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip emptySound;
    public AudioClip weaponPickup;
    [Range(0f, 1f)] public float shootVolume = 0.2f;
    [Range(0f, 1f)] public float emptyVolume = 0.2f;
    [Range(0f, 1f)] public float weaponVolume = 0.2f;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;
}

using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public GameObject bulletPrefab;
    public float fireRate = 0.15f;
    public int maxAmmo = 30;

    // —сылка на уникальные анимации дл€ этой пушки
    [Header("Animations")]
    public RuntimeAnimatorController weaponOverride;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip emptySound;
    public AudioClip weaponPickup;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;
}

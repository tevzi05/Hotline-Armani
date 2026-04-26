//using UnityEngine;
//using System.Collections;

//public class SpawnManager : MonoBehaviour
//{
//    [Header("Weapons")]
//    public GameObject[] weaponPrefabs;        // префабы разных пушек (каждый с компонентом WeaponPickup)
//    public float weaponSpawnInterval = 180f;  // 3 минуты

//    [Header("Ammo")]
//    public GameObject ammoPrefab;             // префаб патронов (с AmmoPickup)
//    public float ammoSpawnInterval = 30f;

//    [Header("Spawn Area")]
//    public Vector2 spawnAreaMin;   // левый нижний угол прямоугольника
//    public Vector2 spawnAreaMax;   // правый верхний угол

//    private void Start()
//    {
//        StartCoroutine(SpawnWeaponsRoutine());
//        StartCoroutine(SpawnAmmoRoutine());
//    }

//    IEnumerator SpawnWeaponsRoutine()
//    {
//        while (true)
//        {
//            yield return new WaitForSeconds(weaponSpawnInterval);
//            SpawnRandomWeapon();
//        }
//    }

//    IEnumerator SpawnAmmoRoutine()
//    {
//        while (true)
//        {
//            yield return new WaitForSeconds(ammoSpawnInterval);
//            SpawnAmmo();
//        }
//    }

//    void SpawnRandomWeapon()
//    {
//        if (weaponPrefabs == null || weaponPrefabs.Length == 0) return;
//        int index = Random.Range(0, weaponPrefabs.Length);
//        GameObject weapon = weaponPrefabs[index];
//        Vector2 pos = GetRandomPosition();
//        Instantiate(weapon, pos, Quaternion.identity);
//    }

//    void SpawnAmmo()
//    {
//        if (ammoPrefab == null) return;
//        Vector2 pos = GetRandomPosition();
//        Instantiate(ammoPrefab, pos, Quaternion.identity);
//    }

//    Vector2 GetRandomPosition()
//    {
//        float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
//        float y = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
//        return new Vector2(x, y);
//    }

//    // Рисуем область спавна в редакторе (для удобства)
//    private void OnDrawGizmosSelected()
//    {
//        Gizmos.color = Color.green;
//        Vector3 center = new Vector3(
//            (spawnAreaMin.x + spawnAreaMax.x) / 2,
//            (spawnAreaMin.y + spawnAreaMax.y) / 2,
//            0
//        );
//        Vector3 size = new Vector3(
//            spawnAreaMax.x - spawnAreaMin.x,
//            spawnAreaMax.y - spawnAreaMin.y,
//            0
//        );
//        Gizmos.DrawWireCube(center, size);
//    }
//}
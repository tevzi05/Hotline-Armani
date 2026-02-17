using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [Header("Movement")]
    [SerializeField] private float movingSpeed = 15f;
    private Rigidbody2D rb;
    private bool isRunning = false;
    
    [Header("Weapon")]
    private bool hasWeapon = false;
    
    [Header("Shooting")]
    [SerializeField] private Transform firePoint;    // Куда перетащить FirePoint
    [SerializeField] private GameObject bulletPrefab; // Префаб пули
    [SerializeField] private float fireRate = 0.5f;
    private float nextFireTime = 0f;
    
    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
    }
    
    private void Update()
    {
        // Стрельба
        if (hasWeapon && Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }
    
    private void FixedUpdate()
    {
        // Движение
        Vector2 input = GameInput.Instance.GetMovementVector().normalized;
        rb.MovePosition(rb.position + input * (Time.fixedDeltaTime * movingSpeed));
        isRunning = input.magnitude > 0.1f;
    }

    private void LateUpdate() // Используем LateUpdate для плавности
    {
        if (hasWeapon)
        {
            // Получаем позицию мыши в мировых координатах
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0; // Обнуляем Z

            // Вычисляем направление от игрока к мыши
            Vector3 direction = mousePos - transform.position;

            // Вычисляем угол поворота
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Поворачиваем FirePoint
            firePoint.rotation = Quaternion.Euler(0, 0, angle);

            // Отражаем спрайт персонажа в зависимости от направления
        }
    }
    private void Shoot()
    {
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }
    
    // Методы для оружия
    public void EquipWeapon() 
    { 
        hasWeapon = true; 
    }
    
    public bool HasWeapon() => hasWeapon;
    public bool IsRunning() => isRunning;
}
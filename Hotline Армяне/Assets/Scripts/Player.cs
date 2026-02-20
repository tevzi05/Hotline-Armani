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


    [Header("Health")]
    [SerializeField] private int maxHealth = 100; // хп персонажа
    private int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth; //ДОБАВЛЕНО

        Instance = this;
        rb = GetComponent<Rigidbody2D>();
    }

    //ДОБАВЛЕНО
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Player health: {currentHealth}");
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player died");
        gameObject.SetActive(false); // или перезапуск сцены
    }
    //КОНЕЦ

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
        if (GameInput.Instance == null) return; // <-- добавить эту строку
        if (rb == null) return; // добавить эту строку

        Vector2 input = GameInput.Instance.GetMovementVector().normalized;
        rb.MovePosition(rb.position + input * (Time.fixedDeltaTime * movingSpeed));
        isRunning = input.magnitude > 0.1f;
    }

    private void LateUpdate()
    {
        if (!hasWeapon) return;               // если нет оружия – выход
        if (GameInput.Instance == null) return; // защита от null
        if (firePoint == null) return;         // добавить проверку на firePoint

        Vector3 mousePos = GameInput.Instance.GetMousePosition();
        // mousePos.z = 0; // убрали, так как GetMousePosition уже возвращает Z=0

        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);
    }

    //private void LateUpdate() // Используем LateUpdate для плавности
    //{
    //    if (hasWeapon)

    //    if (GameInput.Instance == null) return; // <-- добавить эту строку

    //    // Получаем позицию мыши в мировых координатах
    //    Vector3 mousePos = GameInput.Instance.GetMousePosition();
    //        //mousePos.z = 0; // Обнуляем Z

    //    // Вычисляем направление от игрока к мыши
    //    Vector3 direction = mousePos - transform.position;

    //    // Вычисляем угол поворота
    //    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

    //    // Поворачиваем FirePoint
    //    firePoint.rotation = Quaternion.Euler(0, 0, angle);

    //    // Отражаем спрайт персонажа в зависимости от направления

    //}
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
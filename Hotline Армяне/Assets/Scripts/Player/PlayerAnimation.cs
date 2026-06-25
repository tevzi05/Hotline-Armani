using UnityEngine; 

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Ссылки на оба типа игрока
    private Player singlePlayer;

    private const string IS_RUNNING = "IsRunning";
    private const string HAS_WEAPON = "HasWeapon";

    private void Awake()
    {
        // Находим скрипты игрока на этом объекте или над ним
        singlePlayer = GetComponentInParent<Player>();
        if (singlePlayer == null) singlePlayer = GetComponentInChildren<Player>();

        // Жесткий поиск Аниматора и Спрайта по всей иерархии персонажа
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator == null) animator = GetComponentInParent<Animator>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInParent<SpriteRenderer>();
    }


    private void Update()
    {
        if (GameInput.Instance == null || animator == null || spriteRenderer == null) return;

        bool isRunning = false;
        bool hasWeapon = false;
        
        if (singlePlayer != null)
        {
            isRunning = singlePlayer.IsRunning();
            hasWeapon = singlePlayer.HasWeapon();
            AdjustPlayerFacingDirection();
        }

        animator.SetBool(IS_RUNNING, isRunning);
        animator.SetBool(HAS_WEAPON, hasWeapon);
    }

    private void AdjustPlayerFacingDirection()
    {
        if (Time.timeScale == 0) return;
        if (GameInput.Instance == null) return;

        Vector3 mousePos = GameInput.Instance.GetMousePosition();
        Vector3 direction = mousePos - transform.position;
        float rotZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ);
    }
}

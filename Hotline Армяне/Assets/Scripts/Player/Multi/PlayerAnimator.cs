using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerController playerController;

    private const string IS_RUNNING = "IsRunning";
    private const string HAS_WEAPON = "HasWeapon";

    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
        if (playerController == null) playerController = GetComponentInChildren<PlayerController>();

        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator == null) animator = GetComponentInParent<Animator>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInParent<SpriteRenderer>();
    }

    private void Update()
    {
        if (InputManager.Instance == null || animator == null || spriteRenderer == null) return;

        bool isRunning = false;
        bool hasWeapon = false;

        if (playerController != null)
        {
            isRunning = playerController.IsRunning();
            hasWeapon = playerController.HasWeapon();

            // Поворот спрайта только для локального игрока (глобально персонаж уже повёрнут через Rigidbody2D)
            if (PlayerController.LocalInstance == playerController)
                AdjustPlayerFacingDirection();
        }

        animator.SetBool(IS_RUNNING, isRunning);
        animator.SetBool(HAS_WEAPON, hasWeapon);
    }

    private void AdjustPlayerFacingDirection()
    {
        if (Time.timeScale == 0) return;
        if (InputManager.Instance == null) return;

        Vector3 mousePos = InputManager.Instance.GetMousePosition();
        Vector3 direction = mousePos - transform.position;
        float rotZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ);
    }
}
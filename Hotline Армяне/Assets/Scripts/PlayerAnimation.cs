using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{

    private Animator animator;

    private SpriteRenderer spriteRenderer;

    private const string IS_RUNNING = "IsRunning";
    private const string HAS_WEAPON = "HasWeapon";

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // «‡˘ËÚ‡ ÓÚ null
        if (Player.Instance == null || GameInput.Instance == null) return;
        if (animator == null || spriteRenderer == null) return;   // ›“” —“–Œ ” ƒŒ¡¿¬»“‹

        animator.SetBool(IS_RUNNING, Player.Instance.IsRunning());
        animator.SetBool(HAS_WEAPON, Player.Instance.HasWeapon());
        AdjustPlayerFacingDirection();
    }

    private void AdjustPlayerFacingDirection()
    {
        if (GameInput.Instance == null) return;

        Vector3 mousePos = GameInput.Instance.GetMousePosition();

        Vector3 direction = mousePos - transform.position;
        float rotZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ);
    }
}

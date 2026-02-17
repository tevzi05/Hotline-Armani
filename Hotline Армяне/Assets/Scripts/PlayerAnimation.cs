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
        animator.SetBool(IS_RUNNING, Player.Instance.IsRunning());
        animator.SetBool(HAS_WEAPON, Player.Instance.HasWeapon());
        AdjustPlayerFacingDirection();
    }

    private void AdjustPlayerFacingDirection()
    {
        Vector3 mousePos = GameInput.Instance.GetMousePosition();

        Vector3 direction = mousePos - transform.position;
        float rotZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ);
    }
}

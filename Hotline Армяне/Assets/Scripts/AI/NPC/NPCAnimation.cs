using Pathfinding;
using UnityEngine;

public class NPCAnimation : MonoBehaviour
{
    private Animator animator;
    private IAstarAI ai;

    private const string IS_RUNNING = "IsRunning";
    private const string HAS_WEAPON = "HasWeapon";

    private void Awake()
    {
        animator = GetComponent<Animator>();
        ai = GetComponent<IAstarAI>();
    }

    private void Update()
    {
        if (animator == null || ai == null) return;
        bool isRunning = ai.velocity.sqrMagnitude > 0.01f && !ai.reachedDestination;
        bool hasWeapon = false;

        animator.SetBool(IS_RUNNING, isRunning);
        animator.SetBool(HAS_WEAPON, hasWeapon);

        if (isRunning)
        {
            AdjustBotFacingDirection();
        }
    }

    private void AdjustBotFacingDirection()
    {
        if (Time.timeScale == 0) return;

        Vector3 moveDirection = ai.velocity;

        if (moveDirection != Vector3.zero)
        {
            float rotZ = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, rotZ);
        }
    }
}

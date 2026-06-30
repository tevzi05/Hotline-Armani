using Pathfinding;
using UnityEngine;

public class ZombieAnimation : MonoBehaviour
{
    private Animator animator;
    private IAstarAI ai; // Компонент навигации

    private const string IS_RUNNING = "IsRunning";

    private void Awake()
    {
        animator = GetComponent<Animator>();
        ai = GetComponent<IAstarAI>();
    }

    private void Update()
    {
        if (animator == null || ai == null) return;
        bool isMoving = ai.velocity.sqrMagnitude > 0.1f;

        animator.SetBool(IS_RUNNING, isMoving);
    }
}

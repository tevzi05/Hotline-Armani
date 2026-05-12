using UnityEngine;
using Pathfinding;

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

        // Проверяем, движется ли зомби прямо сейчас
        // sqrMagnitude > 0.1f значит, что скорость больше нуля
        bool isMoving = ai.velocity.sqrMagnitude > 0.1f;

        // Передаем состояние в аниматор
        animator.SetBool(IS_RUNNING, isMoving);
    }
}

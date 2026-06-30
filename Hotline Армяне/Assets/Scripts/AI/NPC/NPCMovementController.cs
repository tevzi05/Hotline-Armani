using UnityEngine;
using Pathfinding;

public class NPCMovementController : MonoBehaviour
{
    private IAstarAI ai;
    private AIDestinationSetter destinationSetter;
    private bool isWalkingAway = false;

    private void Awake()
    {
        ai = GetComponent<IAstarAI>();
        destinationSetter = GetComponent<AIDestinationSetter>();

        // При старте игры выключаем движение, чтобы чел стоял и ждал пока с ним побазарят
        if (ai != null)
        {
            ai.canMove = false;
        }
    }

    private void Update()
    {
        if (!isWalkingAway || ai == null) return;

        if (!ai.pathPending && ai.reachedDestination)
        {
            OnReachedDestination();
        }
    }

    // Этот метод вызывается из DialogueManager
    public void StartWalkingAway()
    {
        if (ai != null && destinationSetter != null && destinationSetter.target != null)
        {
            isWalkingAway = true;
            ai.canMove = true;
            Debug.Log("Бот закончил диалог и пошел к цели через A*.");
        }
    }

    private void OnReachedDestination()
    {
        isWalkingAway = false;
        ai.canMove = false; 
        Debug.Log("Бот успешно пришел в точку!");
    }
}

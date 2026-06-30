using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public List<Dialogue> dialogueStages = new List<Dialogue>();
    public int currentStageIndex = 0;
    private bool playerInRange = false;
    private NPCMovementController botController;

    private void Awake()
    {
        botController = GetComponent<NPCMovementController>() ?? GetComponentInChildren<NPCMovementController>() ?? GetComponentInParent<NPCMovementController>();
    }
    private void Update()
    {

        if (playerInRange && Input.GetKeyDown(KeyCode.Space))
        {

            DialogueManager manager = FindObjectOfType<DialogueManager>();
            if (manager != null)
            {

                if (manager != null && !manager.isDialogueActive)
                {
                    if(currentStageIndex < dialogueStages.Count)
                        manager.StartDialogue(dialogueStages[currentStageIndex],this, botController);
                }
            }
        }
    }

    public void OnCurrentDialogueEnded()
    {
        // Проверяем, какой именно диалог сейчас завершился
        Dialogue finishedDialogue = dialogueStages[currentStageIndex];

        if (finishedDialogue.dialogueID == "Встреча")
        {
            // Сценарий 1: Первое общение завершено. Бот просто идет к новой задаче
            if (botController != null)
            {
                botController.StartWalkingAway();
            }
        }
        else if (finishedDialogue.dialogueID == "У_Объекта")
        {
            // Сценарий 2: Дошли до цели, поговорили второй раз
            TriggerGlobalWaveSystem();
        }

        // Переключаем индекс на следующий диалог для будущих встреч
        currentStageIndex++;
    }

    private void TriggerGlobalWaveSystem()
    { 
        if (WaveMusicController.Instance != null) WaveMusicController.Instance.StartNextWaveMusic(1);
        if (WavesManager.Instance != null) WavesManager.Instance.StartSurvivalMode();

        GameObject objective = GameObject.FindGameObjectWithTag("Objective");
        if (objective != null) objective.SetActive(false);
        this.enabled = false;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }
}

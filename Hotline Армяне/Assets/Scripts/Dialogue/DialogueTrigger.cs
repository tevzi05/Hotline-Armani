using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
    private bool playerInRange = false; // Помнит, рядом ли игрок

    private void Update()
    {
        // Нажимаем Пробел, только когда игрок РЯДОМ
        if (playerInRange && Input.GetKeyDown(KeyCode.Space))
        {
            DialogueManager manager = FindObjectOfType<DialogueManager>();
            if (manager != null)
            {
  
                if (manager.isDialogueActive == false)
                {
                    manager.StartDialogue(dialogue);
                }
            }
        }
    }

    // Физическая зона проверки (наступил в круг)
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    // Физическая зона проверки (вышел из круга)
    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // Если игрок ушел посреди разговора — принудительно закрываем
            DialogueManager manager = FindObjectOfType<DialogueManager>();
            if (manager != null)
            {
                manager.EndDialogue();
            }
        }
    }
}

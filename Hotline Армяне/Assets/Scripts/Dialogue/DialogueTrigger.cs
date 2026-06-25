using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
    private bool playerInRange = false;
    private bool hasSpoken = false;
    private void Update()
    {

        if (!hasSpoken && playerInRange && Input.GetKeyDown(KeyCode.Space))
        {

            DialogueManager manager = FindObjectOfType<DialogueManager>();
            if (manager != null)
            {

                if (manager.isDialogueActive == false)
                {
                    manager.StartDialogue(dialogue);
                    hasSpoken=true;
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
}

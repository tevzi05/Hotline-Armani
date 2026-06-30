using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Элементы (TextMeshPro)")]
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI nameText;

    [Header("Активатор интерфейса")]
    public DialogueActivator activator;

    [HideInInspector] public bool isDialogueActive = false;
    public Animator boxAnim;
    [SerializeField] private float textSpeed = 0.03f;

    private Queue<DialogueLine> dialogueLines;
    private bool isTyping = false;
    private string currentCompleteSentence = "";

    private DialogueTrigger currentTrigger; // Ссылка на триггер, который запустил диалог
    private NPCMovementController currentTalkingBot;

    private void Start()
    {
        dialogueLines = new Queue<DialogueLine>();
    }

    private void Update()
    {
        if (boxAnim.GetBool("startOpen") && Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                messageText.text = currentCompleteSentence;
                isTyping = false;
            }
            else
            {
                DisplayNextSentence();
            }
        }
    }

    // Принимает Dialogue, DialogueTrigger и контроллер
    public void StartDialogue(Dialogue dialogue, DialogueTrigger trigger, NPCMovementController npc = null)
    {
        isDialogueActive = true;
        currentTrigger = trigger;
        currentTalkingBot = npc;

        if (Player.Instance != null) Player.Instance.SetDialogueLock(true);
        if (activator != null) activator.DialogueStart();
        if (boxAnim != null) boxAnim.SetBool("startOpen", true);

        dialogueLines.Clear();

        // Заполняем очередь структурами реплик
        foreach (DialogueLine line in dialogue.lines)
        {
            dialogueLines.Enqueue(line);
        }

        StopAllCoroutines();
        StartCoroutine(WaitAndStartFirstSentence(0.1f));
    }

    private IEnumerator WaitAndStartFirstSentence(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (dialogueLines.Count == 0)
        {
            EndDialogue();
            return;
        }

        // Достаем из очереди целую линию
        DialogueLine currentLine = dialogueLines.Dequeue();

        
        nameText.text = currentLine.speakerName;

        currentCompleteSentence = currentLine.text;

        if (currentLine.triggerNextObjectiveAfterThisLine)
        {
            if (ObjectivesManager.Instance != null)
            {
                ObjectivesManager.Instance.CompleteCurrentObjective();
            }
        }

        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentCompleteSentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        messageText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            messageText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        messageText.text = "";
        nameText.text = "";

        if (Player.Instance != null) Player.Instance.SetDialogueLock(false);
        if (boxAnim != null) boxAnim.SetBool("startOpen", false);
        if (activator != null) activator.DialogueEnd();

        // Оповещаем триггер, что этот конкретный диалог завершен
        if (currentTrigger != null)
        {
            currentTrigger.OnCurrentDialogueEnded();
            currentTrigger = null;

        }

        currentTalkingBot = null;
    }
}

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Элементы (TextMeshPro)")]
    public TextMeshProUGUI messageText; // Текст самой реплики
    public TextMeshProUGUI nameText;    // Текст имени

    [Header("Активатор интерфейса")]
    public DialogueActivator activator;

    [HideInInspector] public bool isDialogueActive = false;

    public Animator boxAnim;
    [SerializeField] private float textSpeed = 0.03f;

    private Queue<string> sentences;
    private bool isTyping = false;
    private string currentCompleteSentence = "";

    private GameObject objective;

    private void Start()
    {
        sentences = new Queue<string>();
        objective = GameObject.FindGameObjectWithTag("Objective");
    }

    private void Update()
    {
        // Если диалог открыт и игрок нажимает Пробел или ЛКМ
        if (boxAnim.GetBool("startOpen") && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            if (isTyping)
            {
                // Если текст еще печатается — мгновенно показываем его целиком
                StopAllCoroutines();
                messageText.text = currentCompleteSentence;
                isTyping = false;
            }
            else
            {
                // Иначе листаем к следующей строчке
                DisplayNextSentence();
            }
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        isDialogueActive = true;

        if (Player.Instance != null)
        {
            Player.Instance.SetDialogueLock(true);
        }

        if (activator != null) activator.DialogueStart();

        if (boxAnim != null)
        {
            boxAnim.SetBool("startOpen", true);
        }
        boxAnim.SetBool("startOpen", true);

        nameText.text = dialogue.name;
        messageText.text = ""; // Очищаем поле реплики перед стартом

        sentences.Clear();

        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
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
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }
        string sentence = sentences.Dequeue();
        currentCompleteSentence = sentence;
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        messageText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            messageText.text += letter;
            // Стандартное время ожидания между буквами
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        messageText.text = "";
        nameText.text = "";

        if (Player.Instance != null)
        {
            Player.Instance.SetDialogueLock(false);
        }

        if (boxAnim != null) boxAnim.SetBool("startOpen", false);

        if (activator != null) activator.DialogueEnd();

        if (WavesManager.Instance != null)
        {
            WavesManager.Instance.StartSurvivalMode();
        }
        if (objective != null) objective.SetActive(false);
    }
}

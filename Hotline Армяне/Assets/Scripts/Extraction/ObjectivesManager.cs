using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ObjectivesManager : MonoBehaviour
{
    public static ObjectivesManager Instance { get; private set; }

    [Header("UI Элемент (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI objectiveText; // Текст UI на экране

    [Header("Список задач по порядку")]
    [SerializeField] private List<string> objectivesList = new List<string>();

    private int currentObjectiveIndex = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateObjectiveUI();
    }

    // Метод переключения на следующую задачу
    public void CompleteCurrentObjective()
    {
        currentObjectiveIndex++;
        UpdateObjectiveUI();
    }


    private void UpdateObjectiveUI()
    {
        if (objectiveText == null) return;

        // Если задачи в списке еще есть — выводим текущую
        if (currentObjectiveIndex < objectivesList.Count)
        {
            objectiveText.text = objectivesList[currentObjectiveIndex];
        }
    }
}

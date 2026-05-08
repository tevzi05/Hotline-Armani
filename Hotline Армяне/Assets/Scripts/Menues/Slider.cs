using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextOSDSlider : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI osdText; // Перетащи сюда текст
    [SerializeField] private string barChar = "\u2588";    // Символ прямоугольника 

    private Slider slider;

    void Awake()
    {
        if (slider == null) slider = GetComponent<Slider>();
        //slider = GetComponent<Slider>();
        // Настройки слайдера
        slider.minValue = 0;
        slider.maxValue = 1;

        // Подписываемся на обновление
        slider.onValueChanged.AddListener(delegate { Redraw(); });

        Redraw();
    }

    public void Redraw()
    {

        // ПРОВЕРКА 1: Если слайдер еще не найден (вызов извне), ищем его
        if (slider == null) slider = GetComponent<Slider>();

        // ПРОВЕРКА 2: Если текста нет в инспекторе или он еще не подгрузился
        if (osdText == null) return;

        string bar = "";
        float val = slider.value; // Используем float, так быстрее

        // Рисуем заполненные ячейки
        for (int i = 0; i < Mathf.RoundToInt(val * 10); i++)
        {
            bar += barChar;
        }

        osdText.text = bar;
        //string bar = "";
        //double val = (double)slider.value;

        //// Рисуем заполненные ячейки
        //for (int i = 0; i < val * 10; i++)
        //{
        //    bar += barChar;
        //}

        //osdText.text = bar;
    }
}

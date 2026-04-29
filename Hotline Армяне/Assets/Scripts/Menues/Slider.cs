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
        slider = GetComponent<Slider>();
        // Настройки слайдера
        slider.minValue = 0;
        slider.maxValue = 1;

        // Подписываемся на обновление
        slider.onValueChanged.AddListener(delegate { Redraw(); });

        Redraw();
    }

    public void Redraw()
    {
        string bar = "";
        double val = (double)slider.value;

        // Рисуем заполненные ячейки
        for (int i = 0; i < val * 10; i++)
        {
            bar += barChar;
        }

        osdText.text = bar;
    }
}

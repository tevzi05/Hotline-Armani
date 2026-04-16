using UnityEngine;
using UnityEngine.UI;

public class UIVerticalRoll : MonoBehaviour
{
    [Header("Настройки движения")]
    public float rollSpeed = 400f; // Скорость в пикселях в секунду
    private RectTransform rectTransform;
    private float height;
    private Vector2 startPos;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;

        // Получаем высоту картинки, чтобы знать, когда она полностью ушла
        height = rectTransform.rect.height;
    }

    void Update()
    {
        // Двигаем вниз (используем unscaledDeltaTime для работы в паузе)
        rectTransform.anchoredPosition += Vector2.down * rollSpeed * Time.unscaledDeltaTime;

        // Если картинка полностью ушла вниз (ниже своей начальной позиции на свою высоту)
        if (rectTransform.anchoredPosition.y <= startPos.y - height)
        {
            // Перебрасываем её вверх
            rectTransform.anchoredPosition = new Vector2(startPos.x, startPos.y + height);
        }
    }
}

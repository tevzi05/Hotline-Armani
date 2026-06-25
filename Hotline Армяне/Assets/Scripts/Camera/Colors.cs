using UnityEngine;

public class Colors : MonoBehaviour
{
    [Header("Настройки цветов")]
    [SerializeField] private Color[] backgroundColors;
    [SerializeField] private float changeSpeed = 0.5f;

    private Camera cam;
    private int currentColorIndex = 0;
    private int nextColorIndex = 1;
    private float colorPercentage = 0f;

    void Start()
    {

        cam = GetComponent<Camera>();
        if (backgroundColors == null || backgroundColors.Length < 2)
        {
            backgroundColors = new Color[]
            {
                new Color(0.2f, 0f, 0.4f), // Темно-фиолетовый
                new Color(0.4f, 0f, 0.2f), // Темно-бордовый
                new Color(0f, 0.2f, 0.4f)  // Темно-синий
            };
        }

        if (cam != null && backgroundColors.Length > 0)
        {
            cam.backgroundColor = backgroundColors[0];
        }
    }

    void Update()
    {
        if (cam == null || backgroundColors.Length < 2) return;

        colorPercentage += Time.unscaledDeltaTime * changeSpeed;


        cam.backgroundColor = Color.Lerp(backgroundColors[currentColorIndex], backgroundColors[nextColorIndex], colorPercentage);

        if (colorPercentage >= 1f)
        {
            colorPercentage = 0f; // Сбрасываем счетчик перехода
            currentColorIndex = nextColorIndex; // Текущим цветом становится тот, к которому пришли

            // Выбираем индекс следующего цвета по кругу
            nextColorIndex = (nextColorIndex + 1) % backgroundColors.Length;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class SimpleBlink : MonoBehaviour
{
    public float interval = 0.5f; // Скорость мигания
    private Image img;
    private float timer;

    void Start()
    {
        img = GetComponent<Image>();
    }

    void Update()
    {
        // Используем unscaledDeltaTime, чтобы мигало даже когда Time.timeScale = 0
        timer += Time.unscaledDeltaTime;

        if (timer >= interval)
        {
            img.enabled = !img.enabled; // Просто выключаем/включаем картинку
            timer = 0;
        }
    }
}
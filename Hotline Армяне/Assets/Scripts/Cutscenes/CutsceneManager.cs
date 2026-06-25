using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    [Header("Настройки UI")]
    [SerializeField] private GameObject cutscenePanel;
    [SerializeField] private Sprite firstImage;
    [SerializeField] private Sprite secondImage;
    [SerializeField] private float changeInterval = 6f; // Интервал между сменой картинок
    [SerializeField] private int totalCycles = 6; // Количество циклов смены картинок
    [SerializeField] private float fadeDuration = 1.0f;

    void Start()
    {
        Transform cutsceneTransform = transform.Find("CutsceneManager");
        if (cutsceneTransform != null) cutsceneTransform.gameObject.SetActive(true);
        StartCoroutine(ExecuteCutscene());
    }

    IEnumerator ExecuteCutscene()
    {
        //if (crosshair != null) crosshair.SetActive(false);
        if (Player.Instance != null)
        {
            Player.Instance.SetDialogueLock(true);
        }
        cutscenePanel.SetActive(true);

        for (int i = 0; i < totalCycles; i++)
        {
            cutscenePanel.GetComponent<Image>().sprite = firstImage;
            yield return new WaitForSeconds(changeInterval);
            cutscenePanel.GetComponent<Image>().sprite = secondImage;
            yield return new WaitForSeconds(changeInterval);
        }

        Animator anim = cutscenePanel.GetComponent<Animator>();

        if (anim != null)
        {

            anim.Play("CutsceneFadeOut");
            yield return new WaitForSeconds(1f);
        }

        if (Player.Instance != null)
        {
            Player.Instance.SetDialogueLock(false);
        }
        cutscenePanel.SetActive(false);
        //if (crosshair != null) crosshair.SetActive(true);

    }
}
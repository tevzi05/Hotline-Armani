using UnityEngine;

public class ResetAnimator : MonoBehaviour
{
    // Перетащите сюда вашу кнопку Level1 в инспекторе
    public Animator level1Animator; 

    void OnEnable()
    {
        if (level1Animator != null)
        {
            // 1. Сбрасываем все триггеры, которые могли остаться активными
            level1Animator.ResetTrigger("OnClick"); 
            
            // 2. Мгновенно перематываем аниматор на самое первое, стандартное состояние
            // Замените "Normal", если ваше стартовое состояние в Animator называется иначе
            level1Animator.Play("Normal", 0, 0f);
            
            // 3. Принудительно обновляем параметры трансформации, чтобы кнопка вернула свой размер/альфу
            level1Animator.Update(0f);
        }
    }
}

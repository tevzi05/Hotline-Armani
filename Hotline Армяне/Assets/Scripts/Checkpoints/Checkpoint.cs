using System;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Этот текст должен появиться при ЛЮБОМ столкновении
        Debug.Log("К коллайдеру чекпоинта прикоснулся объект: " + other.name);

        if (other.CompareTag("Player"))
        {
            RestartManager.Instance.SetCheckpoint(transform.position);
            Debug.Log("чекпоинт активирован для игрока!");
            GetComponent<Collider2D>().enabled = false;
        }
    }
}

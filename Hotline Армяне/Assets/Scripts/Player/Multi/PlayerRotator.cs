using UnityEngine;

public class PlayerRotator : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("PlayerRotator is deprecated, rotation handled by PlayerController");
        enabled = false;
    }
}
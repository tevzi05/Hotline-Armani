using UnityEngine;

public class PlayerRotation : MonoBehaviour
{
    private GameInput gameInput;

    private void Start()
    {
        gameInput = GameInput.Instance;
    }

    private void Update()
    {
        HandleRotation();
    }

    private void HandleRotation()
    {
        if (gameInput == null) return; // защита
        Vector3 mousePosition = gameInput.GetMousePosition();
        Vector3 direction = mousePosition - transform.position;

        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
using UnityEngine;
using Unity.Netcode; // Добавьте в самый верх

public class PlayerRotation : MonoBehaviour
{
    private GameInput gameInput;
    private NetworkObject networkObject; // Ссылка на сетевой маркер

    private void Start()
    {
        gameInput = GameInput.Instance;
        networkObject = GetComponent<NetworkObject>();
    }

    private void Update()
    {
        // Если объект сетевой, но он НЕ принадлежит нам — не крутим его мышкой!
        if (networkObject != null && networkObject.IsSpawned && !networkObject.IsOwner) return;

        HandleRotation();
    }

    private void HandleRotation()
    {
        if (gameInput == null) return;
        Vector3 mousePosition = gameInput.GetMousePosition();
        Vector3 direction = mousePosition - transform.position;

        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}

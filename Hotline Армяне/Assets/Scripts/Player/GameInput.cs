using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
   

    public static GameInput Instance { get; private set; }

    private InputSystem_Actions playerInputActions; // многоплатформенность/разная система управления
    private void Awake()
    {
        // Если экземпляр уже существует и это не мы – уничтожаем новый объект
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;

        DontDestroyOnLoad(gameObject); // Сохраняем при смене сцен

        playerInputActions = new InputSystem_Actions();
        playerInputActions.Enable();
    }

    public Vector2 GetMovementVector()
    {
        // Защита от случая, если playerInputActions ещё не инициализирован
        if (playerInputActions == null) return Vector2.zero;
        return playerInputActions.Player.Move.ReadValue<Vector2>();
        //Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>(); //считывание вектора направления игрока
        //return inputVector;
    }

    public Vector3 GetMousePosition()
    {
        // Защита от отсутствия камеры
        if (Camera.main == null) return Vector3.zero;

        // Создаём луч из камеры через позицию мыши
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Определяем плоскость игры (например, Z = 0) с нормалью (0,0,1)
        Plane plane = new Plane(Vector3.forward, Vector3.zero);

        // Находим точку пересечения луча с плоскостью
        float distance;
        if (plane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }

        // Если пересечения нет (например, луч параллелен плоскости), возвращаем позицию персонажа или ноль
        return Vector3.zero;
    }

    private void OnDestroy()
    {
        if (playerInputActions != null)
        {
            playerInputActions.Disable();
            playerInputActions.Dispose();
        }
    }
}

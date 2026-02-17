using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    
    public static GameInput Instance { get; private set; }

    private InputSystem_Actions playerInputActions; // многоплатформенность/разная система управления
    private void Awake()
    {
        Instance = this;
        playerInputActions = new InputSystem_Actions();
        playerInputActions.Enable();
    }

    public Vector2 GetMovementVector()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>(); //считывание вектора направления игрока
        return inputVector;
    }

    public Vector3 GetMousePosition()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return mousePos;
    }
}

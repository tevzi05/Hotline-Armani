using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private InputSystem_Actions playerInputActions;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        playerInputActions = new InputSystem_Actions();
        playerInputActions.Enable();
    }

    public Vector2 GetMovementVector()
    {
        if (playerInputActions == null) return Vector2.zero;
        return playerInputActions.Player.Move.ReadValue<Vector2>();
    }

    public Vector3 GetMousePosition()
    {
        if (Camera.main == null) return Vector3.zero;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.forward, Vector3.zero);
        float distance;
        if (plane.Raycast(ray, out distance))
            return ray.GetPoint(distance);
        return Vector3.zero;
    }

    public bool IsShootingPressed()
    {
        return Mouse.current.leftButton.isPressed;
    }

    public bool IsReloadPressed()
    {
        return Keyboard.current.rKey.wasPressedThisFrame;
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
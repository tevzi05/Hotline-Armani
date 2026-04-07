using UnityEngine;

public class CrosshairUIScript : MonoBehaviour
{
    void OnEnable()
    {
        Cursor.visible = false;
    }

    void Update()
    {
        transform.position = Input.mousePosition;
    }

    void OnDisable()
    {
        Cursor.visible = true;
    }
}

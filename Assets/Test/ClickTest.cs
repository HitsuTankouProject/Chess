using UnityEngine;
using UnityEngine.InputSystem;

public class ClickTest : MonoBehaviour
{
    private int buttonLayer;
    private void Awake()
    {
        buttonLayer = LayerMask.GetMask("Button", "UI", "Chess");
    }


    void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Debug.Log("Right mouse button pressed");

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, buttonLayer, QueryTriggerInteraction.Collide))
            {
                Debug.Log($"Hit {hit.collider.name}");
                
            }
        }
    }
}

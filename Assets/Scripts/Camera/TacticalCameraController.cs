using UnityEngine;
using UnityEngine.InputSystem;

public class TacticalCameraController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private float edgeScrollSize = 15f;
    [SerializeField] private float smoothTime = 0.1f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minZoom = 8f;
    [SerializeField] private float maxZoom = 25f;

    [Header("Bounds")]
    [SerializeField] private GridManager grid;
    [SerializeField] private float boundaryPadding = 2f;

    private Vector3 targetPosition;
    private Vector3 velocity;

    void Start()
    {
        targetPosition = transform.position;
    }

    void Update()
    {
        HandleMovement();
        HandleZoom();

        // Smooth movement
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }

    void HandleMovement()
    {
        Vector3 input = Vector3.zero;

        // WASD
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) input += Vector3.forward;
            if (Keyboard.current.sKey.isPressed) input += Vector3.back;
            if (Keyboard.current.aKey.isPressed) input += Vector3.left;
            if (Keyboard.current.dKey.isPressed) input += Vector3.right;
        }

        // Edge scroll
        if (Mouse.current != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            if (mousePos.x < edgeScrollSize) input += Vector3.left;
            if (mousePos.x > screenWidth - edgeScrollSize) input += Vector3.right;
            if (mousePos.y < edgeScrollSize) input += Vector3.back;
            if (mousePos.y > screenHeight - edgeScrollSize) input += Vector3.forward;
        }

        if (input != Vector3.zero)
{
            input.Normalize();

            // Move relative to rig orientation (ignores vertical tilt)
            Vector3 forward = transform.forward; forward.y = 0f; forward.Normalize();
            Vector3 right = transform.right; right.y = 0f; right.Normalize();

            Vector3 move = (right * input.x + forward * input.z) * moveSpeed * Time.deltaTime;
            targetPosition += move;

            ClampToBounds();
        }
        // Middle mouse drag
        if (Mouse.current != null && Mouse.current.middleButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            targetPosition -= new Vector3(delta.x, 0, delta.y) * 0.02f;
        }
    }

    void HandleZoom()
    {
        if (Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetPosition.y -= scroll * zoomSpeed * Time.deltaTime;
            targetPosition.y = Mathf.Clamp(targetPosition.y, minZoom, maxZoom);
        }
    }

    void ClampToBounds()
    {
        if (grid == null) return;

        float maxX = grid.Width * grid.TileSize;
        float maxZ = grid.Height * grid.TileSize;

        targetPosition.x = Mathf.Clamp(targetPosition.x, -boundaryPadding, maxX + boundaryPadding);
        targetPosition.z = Mathf.Clamp(targetPosition.z, -boundaryPadding, maxZ + boundaryPadding);
    }

    public void FocusOn(Vector3 worldPos)
    {
        targetPosition.x = worldPos.x;
        targetPosition.z = worldPos.z;
        ClampToBounds();
    }
}

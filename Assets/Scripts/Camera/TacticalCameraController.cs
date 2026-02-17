using UnityEngine;
using UnityEngine.InputSystem;

public class TacticalCameraController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private float edgeScrollSize = 15f;

    [Tooltip("General smoothing for free movement.")]
    [SerializeField] private float smoothTime = 0.12f;

    [Tooltip("Snappier smoothing used right after FocusOn().")]
    [SerializeField] private float focusSmoothTime = 0.06f;

    [Header("Rotation (Q/E snap)")]
    [SerializeField] private float rotationSmoothTime = 0.10f;
    [SerializeField] private float rotationStepDegrees = 90f;

    [Header("Zoom (boom distance)")]
    [SerializeField] private Transform cameraTransform; // assign Main Camera transform
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minDistance = 10f;
    [SerializeField] private float maxDistance = 30f;

    [Header("Bounds")]
    [SerializeField] private GridManager grid;
    [SerializeField] private float boundaryPadding = 2f;

    private Vector3 targetPosition;
    private Vector3 positionVelocity;

    private float targetYaw;
    private float yawVelocity;

    private float targetDistance = 20f;

    // When we recently called FocusOn, we use tighter smoothing for a moment
    private float focusBoostTimer = 0f;
    [SerializeField] private float focusBoostDuration = 0.25f;

    void Start()
    {
        targetPosition = transform.position;

        if (cameraTransform == null)
            cameraTransform = Camera.main != null ? Camera.main.transform : null;

        if (cameraTransform != null)
            targetDistance = Mathf.Abs(cameraTransform.localPosition.z);

        targetYaw = transform.eulerAngles.y;
    }

    void Update()
    {
        HandleRotationSnap();
        HandleMovement();
        HandleZoom();

        // Choose smoothing time
        float usedSmooth = (focusBoostTimer > 0f) ? focusSmoothTime : smoothTime;
        if (focusBoostTimer > 0f) focusBoostTimer -= Time.deltaTime;

        // Smooth position
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref positionVelocity,
            usedSmooth
        );

        // Smooth rotation (yaw only)
        float newYaw = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            targetYaw,
            ref yawVelocity,
            rotationSmoothTime
        );
        transform.rotation = Quaternion.Euler(45f, newYaw, 0f); // keep fixed tilt at 45

        // Apply zoom (camera local Z is negative)
        if (cameraTransform != null)
        {
            Vector3 lp = cameraTransform.localPosition;
            lp.z = -targetDistance;
            cameraTransform.localPosition = lp;
        }
    }

    void HandleRotationSnap()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            targetYaw -= rotationStepDegrees;
        }
        else if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            targetYaw += rotationStepDegrees;
        }

        // keep in a nice range (not required, but tidy)
        targetYaw = NormalizeAngle(targetYaw);
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
            float w = Screen.width;
            float h = Screen.height;

            if (mousePos.x < edgeScrollSize) input += Vector3.left;
            if (mousePos.x > w - edgeScrollSize) input += Vector3.right;
            if (mousePos.y < edgeScrollSize) input += Vector3.back;
            if (mousePos.y > h - edgeScrollSize) input += Vector3.forward;
        }

        if (input != Vector3.zero)
        {
            input.Normalize();

            // Move relative to current yaw (ignoring tilt)
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

            // Drag should respect camera yaw too
            Vector3 forward = transform.forward; forward.y = 0f; forward.Normalize();
            Vector3 right = transform.right; right.y = 0f; right.Normalize();

            targetPosition -= right * (delta.x * 0.02f);
            targetPosition -= forward * (delta.y * 0.02f);

            ClampToBounds();
        }
    }

    void HandleZoom()
    {
        if (Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        targetDistance -= scroll * zoomSpeed * Time.deltaTime;
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
    }

    void ClampToBounds()
    {
        if (grid == null) return;

        float maxX = grid.Width * grid.TileSize;
        float maxZ = grid.Height * grid.TileSize;

        targetPosition.x = Mathf.Clamp(targetPosition.x, -boundaryPadding, maxX + boundaryPadding);
        targetPosition.z = Mathf.Clamp(targetPosition.z, -boundaryPadding, maxZ + boundaryPadding);

        // pivot stays on ground plane
        targetPosition.y = 0f;
    }

    public void FocusOn(Vector3 worldPos)
    {
        targetPosition.x = worldPos.x;
        targetPosition.z = worldPos.z;
        ClampToBounds();

        // Make the next quarter-second feel snappier
        focusBoostTimer = focusBoostDuration;
    }

    private static float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0f) angle += 360f;
        return angle;
    }
}

using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [Header("Movement")]
    public float baseMoveSpeed = 10f;
    public float edgeThickness = 10f;
    public Vector2 movementBoundsX = new Vector2(-50, 50);
    public Vector2 movementBoundsZ = new Vector2(-50, 50);

    [Header("Zoom")]
    public float zoomSpeed = 5f;
    public float minZoom = 10f;
    public float maxZoom = 50f;

    [Header("Tilt")]
    public float baseTiltX = 90f;
    public float baseTiltY = 0f;
    public float maxTiltUp = 30f;
    public float maxTiltDown = 10f;
    public float maxTiltLeft = 10f;
    public float maxTiltRight = 10f;
    public float tiltSpeed = 5f;

    [Header("Acceleration")]
    public float accelerationTime = 1f;
    private float movementTimer = 0f;
    private float currentSpeedFactor = 0f;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (UiControllerCreateSessionGameScene.IsMenuActive || UiControllerMainGameScene.IsMenuActive)
            return;

        Vector3 newPosition = transform.position;
        Vector3 mousePosition = Input.mousePosition;

        float zoomFactor = Mathf.InverseLerp(minZoom, maxZoom, newPosition.y);
        float moveSpeed = Mathf.Lerp(baseMoveSpeed / 2, baseMoveSpeed * 2, zoomFactor);

        // Movement direction flags using edge detection inside the screen
        bool moveForward = mousePosition.y >= Screen.height - edgeThickness;
        bool moveBackward = mousePosition.y <= edgeThickness;
        bool moveRight = mousePosition.x >= Screen.width - edgeThickness;
        bool moveLeft = mousePosition.x <= edgeThickness;

        bool isMoving = moveForward || moveBackward || moveRight || moveLeft;

        // Acceleration
        if (isMoving)
        {
            movementTimer += Time.deltaTime;
            currentSpeedFactor = Mathf.Clamp01(movementTimer / accelerationTime);
        }
        else
        {
            movementTimer = 0f;
            currentSpeedFactor = 0f;
        }

        float actualMoveSpeed = moveSpeed * currentSpeedFactor;

        // Apply movement
        if (moveForward) newPosition.z += actualMoveSpeed * Time.deltaTime;
        if (moveBackward) newPosition.z -= actualMoveSpeed * Time.deltaTime;
        if (moveRight) newPosition.x += actualMoveSpeed * Time.deltaTime;
        if (moveLeft) newPosition.x -= actualMoveSpeed * Time.deltaTime;

        // Clamp movement to bounds
        newPosition.x = Mathf.Clamp(newPosition.x, movementBoundsX.x, movementBoundsX.y);
        newPosition.z = Mathf.Clamp(newPosition.z, movementBoundsZ.x, movementBoundsZ.y);

        // Zoom
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0f)
        {
            newPosition.y -= scrollInput * zoomSpeed;
            newPosition.y = Mathf.Clamp(newPosition.y, minZoom, maxZoom);
        }

        // ====== Tilt based on overshoot within edge bounds ======
        float tiltX = 0f;
        float tiltY = 0f;

        if (moveForward)
        {
            float overshoot = Mathf.InverseLerp(Screen.height - edgeThickness, Screen.height, mousePosition.y);
            tiltX = -overshoot * maxTiltUp;
        }
        else if (moveBackward)
        {
            float overshoot = Mathf.InverseLerp(edgeThickness, 0, mousePosition.y);
            tiltX = overshoot * maxTiltDown;
        }

        if (moveRight)
        {
            float overshoot = Mathf.InverseLerp(Screen.width - edgeThickness, Screen.width, mousePosition.x);
            tiltY = overshoot * maxTiltRight;
        }
        else if (moveLeft)
        {
            float overshoot = Mathf.InverseLerp(edgeThickness, 0, mousePosition.x);
            tiltY = -overshoot * maxTiltLeft;
        }

        Quaternion targetRotation = Quaternion.Euler(baseTiltX + tiltX, baseTiltY + tiltY, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * tiltSpeed);

        transform.position = newPosition;
    }
}
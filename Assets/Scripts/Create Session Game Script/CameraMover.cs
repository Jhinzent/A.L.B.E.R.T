using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [Header("Movement")]
    public float baseMoveSpeed = 10f;
    public float edgeThickness = 10f;

    // Separate positive/negative constraints for X and Z
    public float constraintPosX = 50f;
    public float constraintNegX = 50f;
    public float constraintPosZ = 50f;
    public float constraintNegZ = 50f;

    // Public Z offset applied at start or map switch
    public float startZOffset = 0f;

    private Vector2 movementBoundsX;
    private Vector2 movementBoundsZ;

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
    public float tiltSpeed = 4f;

    [Header("Acceleration")]
    public float accelerationTime = 1f;
    private float movementTimer = 0f;
    private float currentSpeedFactor = 0f;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        // Initialize bounds and position at start with no map (gamemaster)
        MoveOverMap(null);
    }

    void Update()
    {
        if (UiControllerCreateSessionGameScene.IsMenuActive || UiControllerMainGameScene.IsMenuActive)
            return;

        Vector3 newPosition = transform.position;
        Vector3 mousePosition = Input.mousePosition;

        float zoomFactor = Mathf.InverseLerp(minZoom, maxZoom, newPosition.y);
        float moveSpeed = Mathf.Lerp(baseMoveSpeed / 2f, baseMoveSpeed * 2f, zoomFactor);

        bool moveForward = mousePosition.y >= Screen.height - edgeThickness;
        bool moveBackward = mousePosition.y <= edgeThickness;
        bool moveRight = mousePosition.x >= Screen.width - edgeThickness;
        bool moveLeft = mousePosition.x <= edgeThickness;

        bool isMoving = moveForward || moveBackward || moveRight || moveLeft;

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

        if (moveForward) newPosition.z += actualMoveSpeed * Time.deltaTime;
        if (moveBackward) newPosition.z -= actualMoveSpeed * Time.deltaTime;
        if (moveRight) newPosition.x += actualMoveSpeed * Time.deltaTime;
        if (moveLeft) newPosition.x -= actualMoveSpeed * Time.deltaTime;

        newPosition.x = Mathf.Clamp(newPosition.x, movementBoundsX.x, movementBoundsX.y);
        newPosition.z = Mathf.Clamp(newPosition.z, movementBoundsZ.x, movementBoundsZ.y);

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0f)
        {
            newPosition.y -= scrollInput * zoomSpeed;
            newPosition.y = Mathf.Clamp(newPosition.y, minZoom, maxZoom);
        }

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

    /// <summary>
    /// Shift movement bounds according to the map position and constraint size.
    /// Also moves camera to center + startZOffset on Z.
    /// </summary>
    public void MoveOverMap(Transform map)
    {
        if (map == null)
        {
            SetMovementBounds(
                new Vector2(-constraintNegX, constraintPosX),
                new Vector2(-constraintNegZ, constraintPosZ)
            );
        }
        else
        {
            Vector3 mapPos = map.position;

            SetMovementBounds(
                new Vector2(mapPos.x - constraintNegX, mapPos.x + constraintPosX),
                new Vector2(mapPos.z - constraintNegZ, mapPos.z + constraintPosZ)
            );
        }

        Vector3 newPos = transform.position;
        newPos.x = (movementBoundsX.x + movementBoundsX.y) / 2f;
        newPos.z = (movementBoundsZ.x + movementBoundsZ.y) / 2f + startZOffset;
        transform.position = newPos;
    }

    public void SetMovementBounds(Vector2 xBounds, Vector2 zBounds)
    {
        movementBoundsX = xBounds;
        movementBoundsZ = zBounds;
    }

    public void SetZoomLimits(float minHeight, float maxHeight)
    {
        minZoom = minHeight;
        maxZoom = maxHeight;
    }
}
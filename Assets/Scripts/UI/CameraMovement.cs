using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Assets.Scripts.Environment;
using System.Collections;

// This is the main script for how the camera moves around
// the script is supposed to be attached to the camera object in unity


public class CameraMovement : MonoBehaviour
{
    public static CameraMovement Instance { get; private set; }
    TileManager tileManager;

    Camera cam = null;
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float maxSpeed = 40f;
    [SerializeField] private float zoomModifer = 3.0f; // final speed multiplication
    [SerializeField] private float cameraBoundsMargin = 10f; // how far outside the island the camera can go before it stops moving
    [Space]
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 40f;
    [SerializeField] private float minZoom = 4f;
    [SerializeField] private float maxZoom = 15f;
    [SerializeField] private float zoomLerpSpeed = 7f; // Higher = faster response, Lower = floatier glide
    [SerializeField] private float minZoomXRotation = 27f; // X rotation when fully zoomed in
    [SerializeField] private float maxZoomXRotation = 35f; // X rotation when fully zoomed out
    [Space]
    [Header("Angle Settings")]
    [SerializeField] private bool allowAngleChange = false;
    [SerializeField] private float angleSpeed = 50f;
    [SerializeField] private float rotationLerpSpeed = 7f; // Higher = faster response, Lower = floatier glide
    [Space]
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset inputActionsAsset; // the input actions asset to use for camera movement
    [SerializeField] private string actionMapName = "Camera"; // name of the action map
    [SerializeField, ReadOnly] private InputAction movementAction;
    [SerializeField, ReadOnly] private InputAction zoomAction;
    [SerializeField, ReadOnly] private InputAction angleAction;
    [Space]
    [Header("Test Pan to point")]
    [SerializeField] private bool testPanToPoint = false;

    private Vector3 currentVelocity = Vector3.zero;
    private float targetZoom; // The desired zoom level based on user input
    private float targetYaw; // The desired angle based on user input

    private bool isLerpingToTarget = false; // Flag to indicate if the camera is currently lerping to a target position


    private void Awake()
    {
        // if we've not assigned input action in the inspector
        if (inputActionsAsset != null)
        {
            var map = inputActionsAsset.FindActionMap(actionMapName);
            if (map != null)
            {
                movementAction = map.FindAction("Movement");
                zoomAction = map.FindAction("Zoom");
                angleAction = map.FindAction("Angle");
            }
        }

        // Fallback: Om ingen asset är dragen, leta efter en genererad klass eller skapa standard (eller skriva ut en varning)
        if (movementAction == null)
        {
            Debug.LogWarning("Could not find Movement action. Please check the action map name ('Camera').");
        }
    }

    private void Start()
    {
        tileManager = TileManager.Instance;
        cam = GetComponent<Camera>();
        cam.nearClipPlane = -50.0f; //we dont clip the camera to the ground
        cam.farClipPlane = 300.0f; 

        if (cam != null)
        {
            targetZoom = cam.orthographicSize;
        }

        targetYaw = transform.eulerAngles.y;

    }

    private void OnEnable()
    {
        movementAction?.Enable();
        zoomAction?.Enable();
        angleAction?.Enable();
    }

    private void OnDisable()
    {
        movementAction?.Disable();
        zoomAction?.Disable();
        angleAction?.Disable();
    }


    private void Update()
    {
        updateCameraMovement();
        updateZoom();

        if (allowAngleChange)
            updateAngle();

        if (testPanToPoint)
        {
            testPanToPoint = false;
            PanToPosition(new Vector3(0, transform.position.y, 0), 1.0f); // Example target position and duration
        }
    }

    private void updateZoom()
    {
        if (zoomAction == null || cam == null)
            return;

        // we read input from inputactions
        float scrollValue = zoomAction.ReadValue<float>();
        if (scrollValue != 0f)
        {
            // Dividing by 120 normalizes standard mouse wheel ticks into clean steps
            float scrollDelta = scrollValue / 120f;

            // Adjust the target zoom based on input
            targetZoom -= scrollDelta * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        // Smoothly glide the cameras real size to target zoom
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, zoomLerpSpeed * Time.deltaTime);

        // Beräkna målobjektivets X-rotation baserat på nuvarande zoom-nivå
        float zoomFactor = Mathf.InverseLerp(minZoom, maxZoom, cam.orthographicSize);
        float targetXRotation = Mathf.Lerp(minZoomXRotation, maxZoomXRotation, zoomFactor);

        float currentXRotation = transform.eulerAngles.x;
        float xDelta = targetXRotation - currentXRotation;

        if (Mathf.Abs(xDelta) > 0.001f)
        {
            // find the point on the island we are looking at (pivot point) via raycast/plane
            Vector3 pivotPoint = GetFocusPoint();
            transform.RotateAround(pivotPoint, transform.right, xDelta);
        }

        // glide the cameras real rotation to target rotation
        float currentYaw = transform.eulerAngles.y;
        float smoothedYaw = Mathf.LerpAngle(currentYaw, targetYaw, rotationLerpSpeed * Time.deltaTime);
        float yawDelta = smoothedYaw - currentYaw;

        if (Mathf.Abs(yawDelta) > 0.001f)
        {
            Vector3 pivotPoint = GetFocusPoint();
            transform.RotateAround(pivotPoint, Vector3.up, yawDelta);
        }
    }


    private void updateCameraMovement()
    {
        if (movementAction == null)
            return;

        // we read input from inputactions
        Vector2 inputVector = movementAction.ReadValue<Vector2>();
        float horizontal = inputVector.x;
        float vertical = inputVector.y;

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        // Project camera directions onto the ground, so it doesn't move up and down
        forward = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
        right = Vector3.ProjectOnPlane(right, Vector3.up).normalized;

        // Combine input and normalize it so diagonal movement isn't faster (length > 1)
        Vector3 inputDirection = (forward * vertical + right * horizontal).normalized;


        // Scale the speed based on how zoomed out we are using a curve (0.0 fully in, to 1.0 fully out)
        float zoomFactor = Mathf.InverseLerp(minZoom, maxZoom, cam.orthographicSize);

        // Squaring the zoomFactor makes it stay closer to 1 when zoomed in, so the camera moves slower and more controlled
        // But ramps up quickly as you zoom out.
        float zoomMultiplier = 1f + (zoomFactor * zoomFactor) * zoomModifer;
        float targetSpeed = moveSpeed * zoomMultiplier;

        // Clamp
        targetSpeed = Mathf.Min(targetSpeed, maxSpeed);

        // Smoothly accelerate/decelerate towards the target velocity
        Vector3 targetVelocity = inputDirection * targetSpeed;
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.deltaTime);

        // Apply movement
        Vector3 position = transform.position;
        position += currentVelocity * Time.deltaTime;

        // make sure the camera doesn't move up or down, that is the zooms job
        position.y = transform.position.y;


        // get the chunk parameters so we can make sure to not go far away from the island
        position = GetClampedPosition(position);

        transform.position = position;

    }


    private void updateAngle()
    {
        if (angleAction == null)
            return;

        // we read input from inputactions
        float rotationInput = angleAction.ReadValue<float>();
        if (rotationInput != 0f)
        {
            targetYaw += rotationInput * angleSpeed * Time.deltaTime;
        }

        float currentYaw = transform.eulerAngles.y;
        float smoothedYaw = Mathf.LerpAngle(currentYaw, targetYaw, rotationLerpSpeed * Time.deltaTime);
        float yawDelta = smoothedYaw - currentYaw;

        if (Mathf.Abs(yawDelta) > 0.001f)
        {
            // find the point on the island we are looking at (pivot point) via raycast/plane
            Vector3 pivotPoint = GetFocusPoint();

            // rotate the camera around the pivot point based on the yaw delta
            transform.RotateAround(pivotPoint, Vector3.up, yawDelta);
        }
    }

    private Vector3 GetFocusPoint()
    {
        // send out a ray from the camera forward and see if it hits anything, if it does return that point
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 300f))
        {
            return hit.point;
        }

        // fallback: if it doesn't hit anything, we can use a plane at y=0 to find a point in front of the camera
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }

        // last fallback: if it doesn't hit anything, just return a point 20 units in front of the camera
        return transform.position + transform.forward * 20f;
    }



    private Vector3 GetClampedPosition(Vector3 position)
    {
        if (tileManager != null)
        {
            int chunkSize = tileManager.ChunkSize;
            Vector2Int gridSize = tileManager.ChunkGridSize;

            float halfWidth = (gridSize.x * chunkSize) / 2f;
            float halfHeight = (gridSize.y * chunkSize) / 2f;

            float minX = -halfWidth;
            float maxX = halfWidth;
            float minZ = -halfHeight;
            float maxZ = halfHeight;

            position.x = Mathf.Clamp(position.x, minX - cameraBoundsMargin, maxX + cameraBoundsMargin);
            position.z = Mathf.Clamp(position.z, minZ - cameraBoundsMargin, maxZ + cameraBoundsMargin);
        }
        return position;
    }


    public void PanToPosition(Vector3 targetWorldPosition, float duration = 0.5f)
    {
        StartCoroutine(LerpCameraRoutine(targetWorldPosition, duration));
    }

    private IEnumerator LerpCameraRoutine(Vector3 targetWorldPosition, float duration = 0.5f)
    {
        isLerpingToTarget = true;
        currentVelocity = Vector3.zero; // Reset velocity so it doesn't continue to slide

        // get the current focus point and the offset from the camera to that point
        Vector3 currentFocus = GetFocusPoint();
        Vector3 cameraOffset = transform.position - currentFocus;

        // get the target world position and clamp it to the island bounds
        targetWorldPosition.y = 0f; // we assume ground is y=0
        targetWorldPosition = GetClampedPosition(targetWorldPosition);

        // calculate the target camera position based on the target world position and the current offset
        Vector3 startPos = transform.position;
        Vector3 targetCameraPos = targetWorldPosition + cameraOffset;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            // Smooth Ease-In-Out curve
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPos, targetCameraPos, t);
            yield return null;
        }

        transform.position = targetCameraPos;
        isLerpingToTarget = false;
    }

}

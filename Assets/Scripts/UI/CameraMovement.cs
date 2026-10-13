using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

// This is the main script for how the camera moves around
// the script is supposed to be attached to the camera object in unity


public class CameraMovement : MonoBehaviour
{

    Camera cam = null;
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float maxSpeed = 40f;
    [SerializeField] private float zoomModifer = 3.0f; // final speed multiplication
    [Space]
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 40f;
    [SerializeField] private float minZoom = 4f;
    [SerializeField] private float maxZoom = 10f;
    [SerializeField] private float zoomLerpSpeed = 7f; // Higher = faster response, Lower = floatier glide
    [Space]
    [Header("Angle Settings")]
    [SerializeField] private bool allowAngleChange = false;
    [SerializeField] private float angleSpeed = 50f;
    [SerializeField] private float rotationLerpSpeed = 7f; // Higher = faster response, Lower = floatier glide


    private Vector3 currentVelocity = Vector3.zero;
    private float targetZoom; // The desired zoom level based on user input
    private float targetYaw; // The desired angle based on user input

    private void Start()
    {
        cam = GetComponent<Camera>();
        cam.nearClipPlane = -50.0f; //we dont clip the camera to the ground
        cam.farClipPlane = 300.0f; 

        if (cam != null)
        {
            targetZoom = cam.orthographicSize;
        }

        targetYaw = transform.eulerAngles.y;

    }

    private void Update()
    {
        updateCameraMovement();
        updateZoom();

        if (allowAngleChange)
            updateAngle();
    }

    private void updateZoom()
    {
        if (Mouse.current == null || cam == null)
            return;

        // current scrollwhell value
        float scrollValue = Mouse.current.scroll.ReadValue().y;

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
    }


    private void updateCameraMovement()
    {
        if (Keyboard.current == null)
            return;

        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.wKey.isPressed)
            vertical += 1f;

        if (Keyboard.current.sKey.isPressed)
            vertical -= 1f;

        if (Keyboard.current.dKey.isPressed)
            horizontal += 1f;

        if (Keyboard.current.aKey.isPressed)
            horizontal -= 1f;

        // early exit if none is pressed
        //if (horizontal == 0f && vertical == 0f)
        //    return;

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

        transform.position = position;
    }



    private void updateAngle()
    {
        if (Keyboard.current == null)
            return;

        float rotationInput = 0f;


        if (Keyboard.current.qKey.isPressed)
            rotationInput -= 1f;         
        
        if (Keyboard.current.eKey.isPressed)
            rotationInput += 1f;

        if (rotationInput != 0f)
        {
            targetYaw += rotationInput * angleSpeed * Time.deltaTime;
        }

        // Smoothly glide the cameras real yaw to targetYaw
        float currentYaw = transform.eulerAngles.y;
        float smoothedYaw = Mathf.LerpAngle(currentYaw, targetYaw, rotationLerpSpeed * Time.deltaTime);

        // Only update Y
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, smoothedYaw, transform.eulerAngles.z);

    }

}

using UnityEngine;
using UnityEngine.InputSystem;

public class MousePositionManager : MonoBehaviour
{
    public static MousePositionManager Instance;

    //Updated the camera to instead of using references to fetch it from the scene
    private Camera mainCamera;

    private Vector3 worldPosition;
    private RaycastHit currentHit;

    public Vector3 WorldPosition => worldPosition;
    public RaycastHit CurrentHit => currentHit;

    private void Awake()
    {
        Instance = this;
        
        //Fetches the first object found with name "Main Camera" - hardcoded but should work as we should always have a camera (Main Camera)
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    private void Update()
    {
        UpdateMouseWorldPosition();
    }

    private void UpdateMouseWorldPosition()
    {
        if (Mouse.current == null || mainCamera == null)
            return;

        Vector2 mouseScreenPosition = Input.mousePosition;

        Ray ray =
            mainCamera.ScreenPointToRay(mouseScreenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            currentHit = hit;
            worldPosition = hit.point;
        }
    }

    public bool TryGetWorldPosition(out Vector3 position)
    {
        if(Mouse.current == null || mainCamera == null)
        {
            position = Vector3.zero;
            return false;
        }

        Vector2 mouseScreenPosition = Input.mousePosition;

        Ray ray =
            mainCamera.ScreenPointToRay(mouseScreenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            position = hit.point;
            return true;
        }

        position = Vector3.zero;
        return false;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(worldPosition, 0.1f);
    }
}

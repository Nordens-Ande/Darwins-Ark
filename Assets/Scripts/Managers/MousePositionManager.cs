using UnityEngine;
using UnityEngine.InputSystem;

public class MousePositionManager : MonoBehaviour
{
    public static MousePositionManager Instance;

    [SerializeField] private Camera mainCamera;

    private Vector3 worldPosition;
    private RaycastHit currentHit;

    public Vector3 WorldPosition => worldPosition;
    public RaycastHit CurrentHit => currentHit;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        UpdateMouseWorldPosition();
    }

    private void UpdateMouseWorldPosition()
    {
        if (Mouse.current == null || mainCamera == null)
            return;

        Vector2 mouseScreenPosition =
            Mouse.current.position.ReadValue();

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

        Vector2 mouseScreenPosition =
            Mouse.current.position.ReadValue();

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
}

using Assets.Scripts.Environment;
using Assets.Scripts.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

public class WindCreationManager : MonoBehaviour
{
    private enum WindCreationState
    {
        None,
        SelectTile,
        SelectDirection
    }

    WindCreationState state = WindCreationState.None;

    private GameManager gameManager;
    private MousePositionManager mousePositionManager;
    private TileManager tileManager;
    private WindDeityManager windDeityManager;

    private Tile tile;

    private Camera mainCamera;

    private bool creatingWind = false;
    private Vector3 initialMousePosition;

    void Start()
    {
        gameManager = GameManager.Instance;
        mousePositionManager = MousePositionManager.Instance;
        tileManager = TileManager.Instance;
        windDeityManager = WindDeityManager.Instance;
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    public void OnCreateWind() // call from ui
    {
        if (state != WindCreationState.None) return;
        if(mainCamera == null)
        {
            Debug.LogWarning("camera not found for wind deity");
            return;
        }

        tile = null;
        
        state = WindCreationState.SelectTile;
    }

    private void HandleSelectTile()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (mousePositionManager == null) Debug.Log("mouseposmanager null. from wind creation manager");

        if (!mousePositionManager.TryGetWorldPosition(out Vector3 pos)) return;

        Tile tile = tileManager.GetTile(pos);
        if (tile == null) return;

        this.tile = tile;
        initialMousePosition = Input.mousePosition;

        state = WindCreationState.SelectDirection;

    }

    private void HandleSelectDirection()
    {
        Vector3 mouseDelta = Input.mousePosition - initialMousePosition;

        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 windDirection = (cameraForward * mouseDelta.y + cameraRight * mouseDelta.x).normalized;

        if (mouseDelta.sqrMagnitude < 1) return;

        if (!Input.GetMouseButtonDown(0)) return;


        windDeityManager.TriggerWind(tile, windDirection);
        tile = null;
        state = WindCreationState.None;
    }

    void Update()
    {
        if(state == WindCreationState.None) return;

        if (state == WindCreationState.SelectTile) HandleSelectTile();
        if (state == WindCreationState.SelectDirection) HandleSelectDirection();
    }
}

using Assets.Scripts.Environment;
using UnityEngine;
using UnityEngine.InputSystem;

public enum TerraformTool
{
    None,
    River//,
         //Mountain
}

public class TerraformManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    private TerraformTool currentTool = TerraformTool.None;

    private void Update()
    {
        if (currentTool == TerraformTool.None)
            return;

        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            SelectTile();
        }
    }

    public void SelectRiverTool()
    {
        currentTool = TerraformTool.River;
        Debug.Log("River tool selected");
    }

    //public void SelectMountainTool()
    //{
    //    currentTool = TerraformTool.Mountain;
    //    Debug.Log("Mountain tool selected");
    //}

    public void ClearTool()
    {
        currentTool = TerraformTool.None;
    }

    private void SelectTile()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Tile tile = TileManager.Instance.GetTile(hit.point);
            Chunk chunk = TileManager.Instance.GetChunk(hit.point);

            if (tile == null || chunk == null)
                return;

            if (currentTool == TerraformTool.River)
            {
                tile.position.y = -0.5f;

            }

            
        }
    }
}
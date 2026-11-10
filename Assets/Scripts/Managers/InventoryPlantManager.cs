using Assets.Scripts.Environment;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// this script does the doesgic for taking plants from the inventory
/// choosing a tile for it to be planted on and then planting it
/// </summary>

public class InventoryPlantManager : SingletonManager<InventoryPlantManager>
{

    private TileManager tileManager;
    private InventoryManager inventoryManager;

    [SerializeField, ReadOnly] private Vector2Int? highlightedTile;
    [SerializeField, ReadOnly] private int selectedInventorySlotIndex;

    private bool isSelectingTileToPlant = false;


    private void Start()
    {
        tileManager = TileManager.Instance;
        inventoryManager = InventoryManager.Instance;

        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager instance could not be found!");
        }

    }


    public void Update()
    {
        // This is temporary code to try the planting system without there being an ui
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            SelectInventorySlot(0);
            Debug.Log("Test: Automatically selected inventory slot 0 for planting.");
        }

        // Press Escape to cancel planting mode
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ResetSelection();
            Debug.Log("Test: Cancelled planting mode.");
        }

        // Handle hover and click polling while selecting a tile
        if (isSelectingTileToPlant)
        {
            HandleMouseHover();

            // Leftclick to plant
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                TryPlantSelectedSeed();
            }
        }

    }


    private void HandleMouseHover()
    {
        if (MousePositionManager.Instance == null)
        {
            Debug.LogWarning("MousePositionManager.Instance is null");
            return;
        }

        if (MousePositionManager.Instance.TryGetWorldPosition(out Vector3 mouseWorldPosition))
        {
            Vector2Int mouseGridPosition = new Vector2Int(Mathf.RoundToInt(mouseWorldPosition.x), Mathf.RoundToInt(mouseWorldPosition.z));
            SetHighlight(mouseGridPosition.x, mouseGridPosition.y);
        }
    }




    // we call this from the ui when we click on a inventory slot
    // it will set the selectedInventorySlotIndex to the index of the slot we clicked on
    public void SelectInventorySlot(int slotIndex) 
    {
        GetComponent<PlayerInput>().SwitchCurrentActionMap("Inventory");

        if (inventoryManager == null || inventoryManager.playerInventory == null)
            return;

        List<InventorySlot> slots = inventoryManager.playerInventory.Slots;
        if (slotIndex < 0 || slotIndex >= slots.Count)
            return;

        InventorySlot slot = slots[slotIndex];
        if (slot.IsEmpty)
        {
            Debug.Log("Selected slot is empty; cannot plant.");
            selectedInventorySlotIndex = -1;
            isSelectingTileToPlant = false;
            return;
        }

        selectedInventorySlotIndex = slotIndex;
        SetHighlightGridActive(true);
        isSelectingTileToPlant = true;
    }

    private void SetHighlightGridActive(bool active)
    {
        if (tileManager == null) return;

        Transform grid = tileManager.transform.Find("Highlight Grid");
        if (grid != null)
            grid.gameObject.SetActive(active);
    }

    private void SetHighlight(int x, int z)
    {
        if (tileManager == null) return;

        Tile newTile = tileManager.GetTile(x, z);
        if (newTile == null) return;

        Vector2Int newPos = newTile.GridPosition;
        if (highlightedTile == newPos) return;

        highlightedTile = newPos;
        tileManager.SetHighlightGridTiles(new HashSet<Vector2Int> { newPos });
    }

    public void TryPlantSelectedSeed()
    {
        if (!highlightedTile.HasValue)
        {
            Debug.Log("You tried to plant while not having a tile highlighted");
            return;
        }

        if (selectedInventorySlotIndex == -1)
        {
            Debug.Log("No inventory slot selected");
            return;
        }

        Tile targetTile = tileManager.GetTile(highlightedTile.Value);
        if (targetTile == null)
        {
            Debug.LogWarning("Highlighted tile does not exist.");
            return;
        }

        if (targetTile.HasPlant)
        {
            Debug.Log("This tile already has a plant.");
            return;
        }

        // we try to plant on the tile
        bool success = inventoryManager.TryPlantFromSlot(selectedInventorySlotIndex, targetTile);

        if (!success)
        {
            Debug.LogWarning("failed to plant seed from the selected slot");
        }

    }





    private void ResetSelection()
    {
        selectedInventorySlotIndex = -1;
        isSelectingTileToPlant = false;
        highlightedTile = null;

        tileManager.SetHighlightGridTiles(new HashSet<Vector2Int>());
        SetHighlightGridActive(false);
        GetComponent<PlayerInput>().SwitchCurrentActionMap("Player");
    }

}

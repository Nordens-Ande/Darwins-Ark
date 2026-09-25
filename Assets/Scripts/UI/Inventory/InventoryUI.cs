using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;

    public void ToggleInventory()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }

    [SerializeField] private GameObject gamePanel;

    public void ToggleButton()
    {
        gamePanel.SetActive(!gamePanel.activeSelf);
    }

}

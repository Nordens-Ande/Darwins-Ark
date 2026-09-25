using UnityEngine;

public class UIButtonToggle : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;

    public void ToggleInventory()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }

    [SerializeField] private GameObject terraformPanel;

    public void ToggleTerraform()
    {
        terraformPanel.SetActive(!terraformPanel.activeSelf);
    }
}

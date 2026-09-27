using UnityEngine;

public class UIButtonToggle : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject terraformPanel;

    [SerializeField] private UIStateManager uiStateManager;



    public void ToggleInventory()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }

    public void EnterTerraform()
    {
        uiStateManager.EnterTerraformMode();
    }

    public void ExitTerraform()
    {
        uiStateManager.ExitTerraformMode();
    }
    public void ToggleBoss()
    {
        BossManager.Instance.SpawnBoss();
    }
}

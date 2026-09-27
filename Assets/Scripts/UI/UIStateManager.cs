
using Assets.Scripts.Environment;
using UnityEngine;

public class UIStateManager : MonoBehaviour
{
    [SerializeField] private GameObject normalUI;
    [SerializeField] private GameObject terraformUI;

    private bool isTerraforming = false;

    private void Start()
    {
        normalUI.SetActive(true);
        terraformUI.SetActive(false);
    }

    public void EnterTerraformMode()
    {
        isTerraforming = true;

        normalUI.SetActive(false);
        terraformUI.SetActive(true);

        Debug.Log("Entered Terraform Mode");

        TileManager.Instance.SetChunkGridVisible(true);
        TileManager.Instance.SetTileGridVisible(true);
    }

    public void ExitTerraformMode()
    {
        isTerraforming = false;

        normalUI.SetActive(true);
        terraformUI.SetActive(false);

        TileManager.Instance.SetChunkGridVisible(false);
        TileManager.Instance.SetTileGridVisible(false);

        Debug.Log("Exited Terraform Mode");
    }

    public bool IsTerraforming()
    {
        return isTerraforming;
    }
}

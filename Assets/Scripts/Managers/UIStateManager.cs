
using Assets.Enums.GameTransitions;
using Assets.Scripts.Environment;
using Assets.Scripts.Managers;
using UnityEngine;

public class UIStateManager : MonoBehaviour
{
    private GameManager gameManager;

    [SerializeField] private GameObject normalUI;
    [SerializeField] private GameObject terraformUI;
    [SerializeField] private TileSelectionManager tileSelectionManager;

    private bool isTerraforming = false;

    private void Start()
    {
        gameManager = GameManager.Instance;

        normalUI.SetActive(true);
        terraformUI.SetActive(false);
    }

    public void EnterTerraformMode()
    {
        isTerraforming = true;

        normalUI.SetActive(false);
        terraformUI.SetActive(true);

        Debug.Log("Entered Terraform Mode");

        //TileManager.Instance.SetChunkGridVisible(true);
        //TileManager.Instance.SetTileGridVisible(true);

        //tileSelectionManager.SetMiddleTile();
        //tileSelectionManager.SetGridVisible(true);

        gameManager.ChangeMode(gameObject, GameMode.Terraform);
    }

    public void ExitTerraformMode()
    {
        isTerraforming = false;

        // TileSelectionManager-------------------
        //tileSelectionManager.ClearSelection();

        normalUI.SetActive(true);
        terraformUI.SetActive(false);

        //TileManager.Instance.SetChunkGridVisible(false);
        //TileManager.Instance.SetTileGridVisible(false);

        //tileSelectionManager.ClearEverything();
        //tileSelectionManager.SetGridVisible(false);

        gameManager.ChangeMode(gameObject, GameMode.Game);

        Debug.Log("Exited Terraform Mode");
    }

    public bool IsTerraforming()
    {
        return isTerraforming;
    }
}

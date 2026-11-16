
using Assets.Enums.GameTransitions;
using Assets.Scripts.Environment;
using Assets.Scripts.Events.Data;
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

        gameManager.OnGameModeChanged += HandleModeChanged;
    }

    private void HandleModeChanged(GameModeChangedData data)
    {
        if(data.NewValue == GameMode.Game)
        {
            normalUI.SetActive(true);
            terraformUI.SetActive(false);
            //disable other ui
        }
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

        //normalUI.SetActive(true);      // now handled by HandleModeChanged() above
        //terraformUI.SetActive(false);

        //TileManager.Instance.SetChunkGridVisible(false);
        //TileManager.Instance.SetTileGridVisible(false);

        //tileSelectionManager.ClearEverything();
        //tileSelectionManager.SetGridVisible(false);

        gameManager.ChangeMode(gameObject, GameMode.Game);

        Debug.Log("Exited Terraform Mode");
    }

    public void EnterWindMode()
    {
        //set to wind ui
        normalUI.SetActive(false);
        gameManager.ChangeMode(gameObject, GameMode.Wind);
    }

    public void ExitWindMode()// for exit button maybe?
    {
        gameManager.ChangeMode(gameObject, GameMode.Game);
    }

    public void EnterLightningMode()
    {
        //set to Lightning ui
        normalUI.SetActive(false);
        gameManager.ChangeMode(gameObject, GameMode.Lightning);
    }

    public void ExitLightningMode()// for exit button maybe?
    {
        gameManager.ChangeMode(gameObject, GameMode.Game);
    }

    public void EnterRainMode()
    {
        //set to rain ui
        normalUI.SetActive(false);
        gameManager.ChangeMode(gameObject, GameMode.Rain);
    }

    public void ExitRainMode() // for exit button maybe?
    {
        gameManager.ChangeMode(gameObject, GameMode.Game);
    }

    public bool IsTerraforming()
    {
        return isTerraforming;
    }
}

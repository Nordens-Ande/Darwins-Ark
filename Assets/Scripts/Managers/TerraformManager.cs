using Assets.Enums.GameTransitions;
using Assets.Scripts.Events.Data;
using Assets.Scripts.Managers;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Environment
{
    public class TerraformManager : SingletonManager<TerraformManager>
    {
        private TileManager tileManager;
        private TileSelectionManager tileSelectionManager;

        private HashSet<Vector2Int> selectedTiles;

        [SerializeField, ReadOnly] private bool isActive = false;
        [SerializeField, ReadOnly] private bool modifySelectionEnabled = false;

        void Start()
        {
            tileManager = TileManager.Instance;
            tileSelectionManager = TileSelectionManager.Instance;
            GameManager gameManager = GameManager.Instance;

            gameManager.OnGameModeChanged += HandleModeChanged;

            HandleModeChanged(new GameModeChangedData(gameObject, GameMode.Game, gameManager.CurrentMode));
        }

        // Update is called once per frame
        void Update()
        {
            if (!isActive)
                return;
        }


        private void HandleModeChanged(GameModeChangedData data)
        {
            isActive = data.NewValue switch
            {
                GameMode.Terraform => true,
                _ => false
            };
        }


        public void OnModifySelection(InputAction.CallbackContext context)
        {
            //Debug.Log($"OnModifySelection called: {context.phase}");
            

            if (context.performed)
            {
                selectedTiles = tileSelectionManager.SelectedTiles;
                modifySelectionEnabled = true;
            }
            else
            {
                modifySelectionEnabled = false;
            }

        }

        public void OnTerraformSelection(InputAction.CallbackContext context)
        {
            //Debug.Log($"OnTerraformSelection called: {context.phase}");

            if (!modifySelectionEnabled)
                return;

            //Debug.Log("We are terraforming");
            Vector2 mouseDelta = context.ReadValue<Vector2>();
            float increment = 10.0f * mouseDelta.y > 0 ? 1f : -1f;

            foreach (Vector2Int tile in selectedTiles)
            {
                tileManager.GetChunk(tile).IncreaseTileHeight(tile, mouseDelta.y * Time.deltaTime);
            }
        }
    }
}

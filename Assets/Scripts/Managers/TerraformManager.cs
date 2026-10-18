using Assets.Enums.GameTransitions;
using Assets.Scripts.Events.Data;
using Assets.Scripts.Managers;
using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Environment
{
    public class TerraformManager : SingletonManager<TerraformManager>
    {
        private TileSelectionManager tileSelectionManager;

        [SerializeField, ReadOnly] bool isActive = false;

        void Start()
        {
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


    }
}

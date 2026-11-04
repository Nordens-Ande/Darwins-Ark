using Assets.Enums.GameTransitions;
using Assets.Scripts.Events.Data;
using Assets.Scripts.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class InputModeController : SingletonManager<InputModeController>
{
    [Serializable]
    private class AlternativeOverride
    {
        public InputActionReference overrideAction;
        public GameMode mode;
        public bool activeUnderOverride;
    }

    private GameManager gameManager;

    [Header("Input Actions")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Alternative controls")]
    [SerializeField] private List<AlternativeOverride> alternativeOverrides = new List<AlternativeOverride>();

    private InputActionMap terraformMap;
    private InputActionMap cameraMap;
    //private InputActionMap itemMap;
    private InputActionMap systemMap;

    // When this button is pressed we enter the alternative control mode, where it may fallback to another subset of controls.
    private InputAction alternativeActionOverride;
    private bool isAlternativeActionActive = false;

    private GameMode currentMode;

    public bool IsAlternativeActionActive => isAlternativeActionActive;
    //public InputActionAsset InputActions => inputActions;

    protected override void Awake()
    {
        base.Awake();

        terraformMap = inputActions.FindActionMap("Terraform", true);
        cameraMap = inputActions.FindActionMap("Camera", true);
        //itemMap = inputActions.FindActionMap("Item", true);
        systemMap = inputActions.FindActionMap("System", true);

        alternativeActionOverride = systemMap.FindAction("AlternativeOverride", true);
    }

    private void OnEnable()
    {
        alternativeActionOverride.performed += OnAlternativeOverrideStart;
        alternativeActionOverride.canceled += OnAlternativeOverrideCanceled;

        systemMap.Enable();
        if (gameManager != null)
            HandleModeChange(new GameModeChangedData(gameObject, GameMode.Game, gameManager.CurrentMode));
    }

    private void OnDisable()
    {
        alternativeActionOverride.performed -= OnAlternativeOverrideStart;
        alternativeActionOverride.canceled -= OnAlternativeOverrideCanceled;

        terraformMap.Disable();
        cameraMap.Disable();
        systemMap.Disable();

        isAlternativeActionActive = false;
    }

    private void Start()
    {
        gameManager = GameManager.Instance;

        gameManager.OnGameModeChanged += HandleModeChange;

        HandleModeChange(new GameModeChangedData(gameObject, GameMode.Game, gameManager.CurrentMode));
    }

    private void Update()
    {
        
    }

    private void UpdateActions()
    {
        switch (currentMode)
        {
            case GameMode.Game:
                cameraMap.Enable();
                terraformMap.Disable();
                break;

            //case GameMode.Deity:

            //    break;

            case GameMode.Terraform:
                cameraMap.Disable();
                terraformMap.Enable();
                break;

            //case GameMode.Planting:

            //    break;
        }

        systemMap.Enable();

        // If the alternative mode is not on, finish the update
        if (!isAlternativeActionActive)
            return;

        foreach (AlternativeOverride entry in alternativeOverrides)
        {
            if (entry == null || entry.overrideAction == null)
                continue;

            if (entry.mode != currentMode)
                continue;

            InputAction action = entry.overrideAction.action;

            if (action == null)
                continue;

            if (entry.activeUnderOverride)
            {
                action.Enable();
            }
            else
            {
                action.Disable();
            }
        }
    }

    public void HandleModeChange(GameModeChangedData data)
    {
        currentMode = data.NewValue;
        //Debug.Log(currentMode);
        //Debug.Log(IsAlternativeActionActive);

        UpdateActions();
    }

    private void OnAlternativeOverrideStart(InputAction.CallbackContext context)
    {
        isAlternativeActionActive = true;
        
        UpdateActions();
    }

    private void OnAlternativeOverrideCanceled(InputAction.CallbackContext context)
    {
        isAlternativeActionActive = false;
        
        UpdateActions();
    }
}

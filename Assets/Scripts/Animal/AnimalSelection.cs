using UnityEngine;
using UnityEngine.InputSystem;

public class AnimalSelection : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    private string actionMapPath = "Selecting";
    private InputAction rightclick;

    private void Awake()
    {
        if (inputActions != null)
        {
            InputActionMap actionMap = inputActions.FindActionMap(actionMapPath);
            if (actionMap != null)
            {
                rightclick = actionMap.FindAction("Selecting");
                if (rightclick == null) 
                {
                    Debug.Log("AnimalSelection: No actions found for rightclick");
                }
            }
            else 
            {
                Debug.Log("AnimalSelection: No actionMap");
            }
        }
        else 
        {
            Debug.Log("AnimalSelection: No inputactions");
        }
    }

    private void OnEnable()
    {
        if (inputActions != null)
        {
            rightclick.Enable();
            rightclick.performed += Select;

        }
    }
    private void OnDisable()
    {
        if (inputActions != null)
        {
            rightclick.performed -= Select;
            rightclick.Disable();
        }
    }


    private void Select(InputAction.CallbackContext context)
    {
        ShotRayFromCamera();
    }

    private void ShotRayFromCamera() 
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if(Physics.Raycast(Camera.main.ScreenPointToRay(mousePos), out RaycastHit hit)) 
        {
            WhatWasHit(hit);
        }
    }

    private void WhatWasHit(RaycastHit hit) 
    { 
        AnimalAI isAnimal = hit.transform.GetComponent<AnimalAI>();
        Popup_customButton isButton = hit.transform.GetComponent<Popup_customButton>();
        if (isAnimal != null) 
        { 
            isAnimal.InstantiatePopUp();
        }
        else if (isButton != null)
        {
            isButton.Onclick();
        }
        else 
        {
            Debug.Log("Hit nothing");
        }
    }
}

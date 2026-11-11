using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;


/// <summary>
/// this script is meant to be a general script to visually selection tool to show what is selected in the UI
/// it is originally meant to be used for the inventory to see what slot is selected when planting
/// but hopefully it can be used for other things as well
///
/// It finds the Canvas in the scene by name, and whenever you click on something inside that canvas
/// it remembers the clicked object and draws the selection marker over it.
/// </summary>


public class GeneralVisualSelection : MonoBehaviour
{
    [SerializeField] Sprite selectionSprite; // image to use for the selection marker
    [SerializeField, ReadOnly] int sortingOrder = 100;

    [Header("What can be selected")]
    [SerializeField] string targetCanvasName = "Canvas"; // name of the canvas gameobject in the scene
    [SerializeField] bool preferClickHandler = true; // if true, when clicking on a button we select the button instead of the text inside it

    [Header("Debug")]
    [SerializeField] bool debug = false; // debug toggle
    [SerializeField] Vector2 debugPosition = new Vector2(950, 600);
    [SerializeField] Vector2 debugSize = new Vector2(500, 200);
    [SerializeField, ReadOnly] string selectedObjectName; // to see what is selected in the inspector, hopefully we can select index of the inventory slot or something like that later

    private Canvas canvas; // the new canvas we create to draw the selection marker on top
    private Canvas targetCanvas; // main ui cavas
    private RectTransform selectionRect; 
    private GameObject activeSelectionInstance;
    private bool debugWasShown = false;

    private GameObject selectedObject; // the currently selected UI object
    private RectTransform selectedRect;

    private readonly Vector3[] corners = new Vector3[4];
    private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

    // other scripts can listen to this to know what got selected (null = nothing selected)
    public event System.Action<GameObject> SelectionChanged;
    public GameObject SelectedObject => selectedObject;




    // on start we create a new canvas to draw the new sprite selection sprite over everything
    void Start()
    {
        // gameobject to hold the canvas component
        GameObject canvasSelectionObject = new GameObject("VisualSelectionCanvas");
        canvasSelectionObject.transform.SetParent(this.transform, false);

        canvas = canvasSelectionObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        // Add the CanvasScaler for responsive UI scaling
        CanvasScaler canvasScaler = canvasSelectionObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        // get the resolution of the screen and set it as the reference resolution for the CanvasScaler
        canvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);

        // The marker must never block clicks on the UI underneath it.
        // That is why there is no GraphicRaycaster here, and why the CanvasGroup doesn't block raycasts.
        CanvasGroup group = canvasSelectionObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;

        if (selectionSprite != null)
        {
            // we build a simple image from the sprite
            activeSelectionInstance = new GameObject("SelectionMarker", typeof(RectTransform), typeof(Image));
            activeSelectionInstance.transform.SetParent(canvas.transform, false);

            Image image = activeSelectionInstance.GetComponent<Image>();
            image.sprite = selectionSprite;
            image.type = Image.Type.Sliced; // if the sprite has 9-slice borders the corners won't stretch
            image.raycastTarget = false;
        }
        else
        {
            Debug.LogWarning("GeneralVisualSelection: assign a selectionSprite.");
            return;
        }

        selectionRect = activeSelectionInstance.GetComponent<RectTransform>();

        // anchored to the bottom left corner of the screen with the pivot in the middle,
        // so anchoredPosition = where the center of the marker is
        selectionRect.anchorMin = Vector2.zero;
        selectionRect.anchorMax = Vector2.zero;
        selectionRect.pivot = new Vector2(0.5f, 0.5f);
        selectionRect.localScale = Vector3.one;

        // Initially hide it until an element is selected
        activeSelectionInstance.SetActive(false);

        FindTargetCanvas();
        if (targetCanvas == null)
        {
            Debug.LogWarning($"GenernalVisualSelection: could not find an active canvas named '{targetCanvasName}' in the scene.");
        }
    }



    private void Update()
    {
        // for debug purposes we can show the selection marker at a fixed position and size, without selecting anything
        if (debug)
        {
            ShowAtCanvasPosition(debugPosition, debugSize);
            debugWasShown = true;
            return;
        }
        else if (debugWasShown)
        {
            Hide();
            debugWasShown = false;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            DetectAndSelectUIElement();
        }
    }


    // LateUpdate so we follow the object after clicked, for example if clikcing a button removes that button
    private void LateUpdate()
    {
        if (debug || selectionRect == null)
            return;

        // nothing selected, or the selected object was destroyed
        if (selectedObject == null)
        {
            if (selectedRect != null)
            {
                ClearSelection();
            }
            return;
        }

        // the inventory panel got closed, we keep the selection but hide the marker
        if (!selectedObject.activeInHierarchy)
        {
            Hide();
            return;
        }

        PositionSelectionOverElement(selectedRect);
    }



    void FindTargetCanvas()
    {
        GameObject found = GameObject.Find(targetCanvasName); // has to be active
        if (found != null)
        {
            targetCanvas = found.GetComponent<Canvas>();
        }
    }


    // check what UI object is under the mouse, and if it's inside our target canvas we select it
    void DetectAndSelectUIElement()
    {
        if (EventSystem.current == null)
            return;

        if (targetCanvas == null)
        {
            FindTargetCanvas(); // try to find it again
            if (targetCanvas == null)
                return;
        }

        // ask the EventSystem what UI is under the mouse
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        if (raycastResults.Count == 0)
            return; // clicked on nothing in the UI

        // we only look at the top most thing that was clicked that is inside our target canvas
        GameObject clicked = raycastResults[0].gameObject;
        if (!clicked.transform.IsChildOf(targetCanvas.transform))
            return;

        // if we click the text on a button we want the button
        if (preferClickHandler)
        {
            GameObject handler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(clicked);
            if (handler != null && handler.transform.IsChildOf(targetCanvas.transform))
            {
                clicked = handler;
            }
        }

        SelectObject(clicked);
    }


    // save the object and put the marker on it
    public void SelectObject(GameObject target)
    {
        RectTransform rect = target != null ? target.GetComponent<RectTransform>() : null;
        if (rect == null)
        {
            ClearSelection();
            return;
        }

        selectedObject = target;
        selectedRect = rect;
        selectedObjectName = target.name;

        PositionSelectionOverElement(selectedRect);
        SelectionChanged?.Invoke(selectedObject);
    }


    public void ClearSelection()
    {
        selectedObject = null;
        selectedRect = null;
        selectedObjectName = "";

        Hide();
        SelectionChanged?.Invoke(null);
    }


    // show the marker at a fixed position, in canvas units
    void ShowAtCanvasPosition(Vector2 position, Vector2 size)
    {
        if (selectionRect == null)
            return;

        activeSelectionInstance.SetActive(true);

        selectionRect.anchoredPosition = position;
        selectionRect.sizeDelta = size;
    }


    public void Hide()
    {
        if (activeSelectionInstance != null)
        {
            activeSelectionInstance.SetActive(false);
        }
    }


    // call this with any UI element and the marker will cover it
    public void PositionSelectionOverElement(RectTransform target)
    {
        if (selectionRect == null || target == null)
            return;

        // if the target is in a camera canvas we need that camera to get the screen position
        Camera targetCamera = null;
        Canvas parentCanvas = target.GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            parentCanvas = parentCanvas.rootCanvas;
            if (parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                targetCamera = parentCanvas.worldCamera;
            }
        }

        // the corners of the target in screen pixels
        target.GetWorldCorners(corners);
        Vector2 min = RectTransformUtility.WorldToScreenPoint(targetCamera, corners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(targetCamera, corners[2]);

        float scale = canvas.scaleFactor;

        activeSelectionInstance.SetActive(true);
        selectionRect.anchoredPosition = ((min + max) * 0.5f) / scale;
        selectionRect.sizeDelta = (max - min) / scale;
    }

}
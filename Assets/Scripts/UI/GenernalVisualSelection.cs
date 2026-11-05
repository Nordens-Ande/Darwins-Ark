using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// this script is meant to be a general script to visually selection tool to show what is selected in the UI
/// it is originally meant to be used for the inventory to see what slot is selected when planting
/// but hopefully it can be used for other things as well
/// </summary>


public class GenernalVisualSelection : MonoBehaviour
{
    [SerializeField] GameObject selectionAssetPrefab;

    private Canvas canvas;
    private RectTransform selectionRect;
    private GameObject activeSelectionInstance;




    // on start we create a new canvas to draw the new sprite selection sprite over everything
    void Start()
    {
        // gameobject to hold the canvas component
        GameObject canvasSelectionObject = new GameObject("VisualSelectionCanvas");

        canvas = canvasSelectionObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Add the CanvasScaler for responsive UI scaling
        CanvasScaler canvasScaler = canvasSelectionObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);

        canvasSelectionObject.AddComponent<GraphicRaycaster>();
        canvasSelectionObject.transform.SetParent(this.transform, false);

        // Instantiate the selection visual inside the canvas if assigned
        if (selectionAssetPrefab != null)
        {
            activeSelectionInstance = Instantiate(selectionAssetPrefab, canvas.transform);
            selectionRect = activeSelectionInstance.GetComponent<RectTransform>();

            // Initially hide it until an element is selected
            activeSelectionInstance.SetActive(false);
        }
    }



    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DetectAndSelectUIElemnt();
        }
    }



    void DetectAndSelectUIElemnt()
    {
        // lowkey i dont know how to do this yet. so we wait a little :skull:


    }


    // draw the rect asset on the right pos
    void PositionSelectionOverElement(RectTransform target)
    {
        if (selectionRect == null)
            return;

        // make the selection active
        activeSelectionInstance.SetActive(true);


        // we position the rect where it is supposed to be
        selectionRect.position = target.position;
        selectionRect.sizeDelta = target.sizeDelta;
        selectionRect.localScale = target.localScale;

    }

}

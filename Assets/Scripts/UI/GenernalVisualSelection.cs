using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// this script is meant to be a general script to visually selection tool to show what is selected in the UI
/// it is originally meant to be used for the inventory to see what slot is selected when planting
/// but hopefully it can be used for other things as well
/// </summary>


public class GenernalVisualSelection : MonoBehaviour
{
    [SerializeField] Sprite selectionSprite; // image to use for the selection marker
    [SerializeField, ReadOnly] int sortingOrder = 100;

    [Header("Debug")]
    [SerializeField] bool debug = false; // debug toggle
    [SerializeField] Vector2 debugPosition = new Vector2(950, 600);
    [SerializeField] Vector2 debugSize = new Vector2(500, 200);

    private Canvas canvas;
    private RectTransform selectionRect;
    private GameObject activeSelectionInstance;
    private bool debugWasShown = false;

    private readonly Vector3[] corners = new Vector3[4];




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
        canvasScaler.referenceResolution = new Vector2(1920, 1080);

        // The marker must never block clicks on the UI underneath it.
        // That is why there is no GraphicRaycaster here, and why the CanvasGroup doesn't block raycasts.
        CanvasGroup group = canvasSelectionObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;

        if (selectionSprite != null)
        {
            // no prefab, so we build a simple Image from the sprite
            activeSelectionInstance = new GameObject("SelectionMarker", typeof(RectTransform), typeof(Image));
            activeSelectionInstance.transform.SetParent(canvas.transform, false);

            Image image = activeSelectionInstance.GetComponent<Image>();
            image.sprite = selectionSprite;
            image.type = Image.Type.Sliced; // if the sprite has 9-slice borders the corners won't stretch
            image.raycastTarget = false;
        }
        else
        {
            Debug.LogWarning("GenernalVisualSelection: assign a selectionAssetPrefab or a selectionSprite.");
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
    }



    private void Update()
    {
        // debug: show the marker at a fixed position so we can see how the png looks
        if (debug)
        {
            ShowAtCanvasPosition(debugPosition, debugSize);
            debugWasShown = true;
        }
        else if (debugWasShown)
        {
            Hide();
            debugWasShown = false;
        }
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


    // draw the rect asset on the right pos
    // call this with any UI element and the marker will cover it
    public void PositionSelectionOverElement(RectTransform target)
    {
        if (selectionRect == null || target == null)
            return;

        // if the target is in a camera canvas we need that camera to get the screen position
        Camera targetCamera = null;
        Canvas targetCanvas = target.GetComponentInParent<Canvas>();
        if (targetCanvas != null)
        {
            targetCanvas = targetCanvas.rootCanvas;
            if (targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                targetCamera = targetCanvas.worldCamera;
            }
        }

        // the corners of the target in screen pixels
        // (this works even if the two canvases use different scaling, unlike copying sizeDelta)
        target.GetWorldCorners(corners);
        Vector2 min = RectTransformUtility.WorldToScreenPoint(targetCamera, corners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(targetCamera, corners[2]);

        // pixels -> units of our own canvas
        float scale = canvas.scaleFactor;

        activeSelectionInstance.SetActive(true);
        selectionRect.anchoredPosition = ((min + max) * 0.5f) / scale;
        selectionRect.sizeDelta = (max - min) / scale;
    }

}

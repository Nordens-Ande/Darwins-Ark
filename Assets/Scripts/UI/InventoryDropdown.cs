
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryDropdown : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform inventoryPanel;
    [SerializeField] private RectTransform itemContainer;
    [SerializeField] private GameObject inventoryItemPrefab;

    [Space]

    [SerializeField] private RectTransform.Axis expandDirection = RectTransform.Axis.Vertical;
    private bool isVertical => expandDirection == RectTransform.Axis.Vertical;
    private bool isHorizontal => expandDirection == RectTransform.Axis.Horizontal;

    private List<string> inventoryItems = new List<string>(); //temporary list to test if functionality works

    [SerializeField, ReadOnly] private bool isOpen = false;

    private Coroutine animation;
    private float targetPos;

    public void Start()
    {
        CreateItemPanel();
        CreateItemContainer();

        inventoryItems.Add("Test");
        inventoryItems.Add("Long Test");
        inventoryItems.Add("Longer Test");

        RefreshInventory();
        inventoryPanel.SetSizeWithCurrentAnchors(expandDirection, 0f);
        inventoryPanel.SetSizeWithCurrentAnchors(expandDirection, 0f);
    }


    private void CreateItemPanel()
    {
        // Object setup
        Type layoutType = isVertical ? typeof(VerticalLayoutGroup) : typeof(HorizontalLayoutGroup);
        inventoryPanel = new GameObject("Item Panel", typeof(RectTransform), typeof(RectMask2D), typeof(ContentSizeFitter), layoutType).GetComponent<RectTransform>();
        inventoryPanel.transform.parent = transform;

        // UI position/alignment setup
        inventoryPanel.anchorMin = isVertical ? new Vector2(0, 1) : new Vector2(0, 0);
        inventoryPanel.anchorMax = isVertical ? new Vector2(1, 1) : new Vector2(0, 1);
        inventoryPanel.pivot     = isVertical ? new Vector2(0, 1) : new Vector2(0, 1);
        inventoryPanel.anchoredPosition = Vector2.zero;
        inventoryPanel.localScale = Vector3.one;

        // Content Size Fitter setup
        ContentSizeFitter fitter = inventoryPanel.GetComponent<ContentSizeFitter>();

        fitter.horizontalFit = !isHorizontal ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = !isVertical ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;

        // Layout Group setup
        if (isVertical)
        {
            VerticalLayoutGroup layout = inventoryPanel.GetComponent<VerticalLayoutGroup>();

            layout.childAlignment = TextAnchor.UpperLeft;

            layout.childControlWidth = true;
            layout.childControlHeight = true;

            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
        }
        else
        {
            HorizontalLayoutGroup layout = inventoryPanel.GetComponent<HorizontalLayoutGroup>();

            layout.childAlignment = TextAnchor.UpperLeft;

            layout.childControlWidth = true;
            layout.childControlHeight = true;

            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
        }
    }
    private void CreateItemContainer()
    {
        if (inventoryPanel == null)
        {
            Debug.LogWarning("Attempting to create Item Container before creating proper Item Panel.\nCreating an item panel now before proceeding with Item Container");
            CreateItemPanel();
        }

        // Object setup
        Type layoutType = isVertical ? typeof(VerticalLayoutGroup) : typeof(HorizontalLayoutGroup);
        itemContainer = new GameObject("Item Container", typeof(ContentSizeFitter), layoutType /*typeof(VerticalLayoutGroup)*/ /*DONT FORGET TO UPDATE THIS*/).GetComponent<RectTransform>();
        itemContainer.transform.parent = inventoryPanel.transform;

        // UI position/alignment setup
        itemContainer.anchorMin = new Vector2(0, 1);
        itemContainer.anchorMax = new Vector2(0, 1);
        itemContainer.pivot     = new Vector2(0, 1);
        itemContainer.anchoredPosition = Vector2.zero;
        itemContainer.localScale = Vector3.one;

        // Content Size Fitter setup
        ContentSizeFitter fitter = itemContainer.GetComponent<ContentSizeFitter>();
        
        fitter.horizontalFit = isHorizontal ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = isVertical ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;

        // Layout Group setup
        if (isVertical)
        {
            VerticalLayoutGroup layout = itemContainer.GetComponent<VerticalLayoutGroup>();

            layout.childAlignment = TextAnchor.UpperLeft;

            layout.childControlWidth  = isHorizontal;
            layout.childControlHeight = isVertical;

            layout.childScaleWidth  = !isHorizontal;
            layout.childScaleHeight = !isVertical;
        }
        else
        {
            HorizontalLayoutGroup layout = itemContainer.GetComponent<HorizontalLayoutGroup>();

            layout.childAlignment = TextAnchor.UpperLeft;

            layout.childControlWidth  = isHorizontal;
            layout.childControlHeight = isVertical;

            layout.childScaleWidth  = !isHorizontal;
            layout.childScaleHeight = !isVertical;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //inventoryPanel.gameObject.SetActive(true);'
        //OpenInventory();
        isOpen = true;
        AnimateInventory();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //inventoryPanel.gameObject.SetActive(false);
        //CloseInventory();
        isOpen = false;
        AnimateInventory();
    }


    private void OpenInventory()
    {
        // Height recalc
        LayoutRebuilder.ForceRebuildLayoutImmediate(itemContainer);

        inventoryPanel.gameObject.SetActive(true);

        Debug.Log(LayoutUtility.GetPreferredHeight(itemContainer));

        inventoryPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, LayoutUtility.GetPreferredHeight(itemContainer));

        RefreshInventory();
    }

    private void CloseInventory()
    {
        //inventoryPanel.gameObject.SetActive(false);

        inventoryPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0f);
    }

    private void AnimateInventory()
    {
        if (animation != null)
            StopCoroutine(animation);

        animation = StartCoroutine(AnimateItems());
    }

    private IEnumerator AnimateItems()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(itemContainer);

        //targetHeight = isOpen ? isVertical ? LayoutUtility.GetPreferredHeight(itemContainer) : LayoutUtility.GetPreferredWidth(itemContainer) : 0f;
        targetPos = expandDirection switch
        {
            RectTransform.Axis.Vertical when isOpen => LayoutUtility.GetPreferredHeight(itemContainer),
            RectTransform.Axis.Horizontal when isOpen => LayoutUtility.GetPreferredWidth(itemContainer),
            _ => 0f
        };

        float startPos = isVertical ? inventoryPanel.rect.height : inventoryPanel.rect.width;
        float elapsed = 0f;
        float animationDuration = 0.25f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float time = Mathf.Clamp01(elapsed / animationDuration);

            // smooth ease animation
            float smoothT = 1f - Mathf.Pow(1f - time, 3f);

            float newPos = Mathf.Lerp(startPos, targetPos, smoothT);

            inventoryPanel.SetSizeWithCurrentAnchors(expandDirection, newPos);

            yield return null;
        }

        inventoryPanel.SetSizeWithCurrentAnchors(expandDirection, targetPos);

        animation = null;
    }

    public void RefreshInventory()
    {
        // Remove old items
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (string item in inventoryItems)
        {
            GameObject newItem = Instantiate(inventoryItemPrefab, itemContainer);

            TMP_Text text = newItem.GetComponentInChildren<TMP_Text>();

            if (text != null)
                text.text = item;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(itemContainer);
    }

}

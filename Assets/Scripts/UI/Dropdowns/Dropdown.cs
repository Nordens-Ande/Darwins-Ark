using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropdownItem
{
    public string text;
    public Image image;
}

public abstract class Dropdown : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, ReadOnly] private RectTransform panel;
    [SerializeField, ReadOnly] private RectTransform itemContainer;
    [SerializeField] private GameObject itemPrefab;

    [Space]

    [SerializeField] private RectTransform.Axis expandDirection = RectTransform.Axis.Vertical;
    private bool isVertical => expandDirection == RectTransform.Axis.Vertical;
    private bool isHorizontal => expandDirection == RectTransform.Axis.Horizontal;

    private List<DropdownItem> items = new List<DropdownItem>();

    [SerializeField, ReadOnly] private bool isOpen = false;

    private Coroutine coroutineAnimation;
    private float targetPos;

    protected virtual void Start()
    {
        CreateItemPanel();
        CreateItemContainer();

        RefreshContainer();
        panel.SetSizeWithCurrentAnchors(expandDirection, 0f);
        panel.SetSizeWithCurrentAnchors(expandDirection, 0f);
    }


    private void CreateItemPanel()
    {
        // Object setup
        Type layoutType = isVertical ? typeof(VerticalLayoutGroup) : typeof(HorizontalLayoutGroup);
        panel = new GameObject("Item Panel", typeof(RectTransform), typeof(RectMask2D), typeof(ContentSizeFitter), layoutType).GetComponent<RectTransform>();
        panel.parent = transform;

        // UI position/alignment setup
        panel.anchorMin = isVertical ? new Vector2(0, 1) : new Vector2(0, 0);
        panel.anchorMax = isVertical ? new Vector2(1, 1) : new Vector2(0, 1);
        panel.pivot = isVertical ? new Vector2(0, 1) : new Vector2(0, 1);
        panel.anchoredPosition = Vector2.zero;
        panel.localScale = Vector3.one;

        // Content Size Fitter setup
        ContentSizeFitter fitter = panel.GetComponent<ContentSizeFitter>();

        fitter.horizontalFit = !isHorizontal ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = !isVertical ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;

        // Layout Group setup
        if (isVertical)
        {
            VerticalLayoutGroup layout = panel.GetComponent<VerticalLayoutGroup>();

            layout.childAlignment = TextAnchor.UpperLeft;

            layout.childControlWidth = true;
            layout.childControlHeight = true;

            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
        }
        else
        {
            HorizontalLayoutGroup layout = panel.GetComponent<HorizontalLayoutGroup>();

            layout.childAlignment = TextAnchor.UpperLeft;

            layout.childControlWidth = true;
            layout.childControlHeight = true;

            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
        }
    }
    private void CreateItemContainer()
    {
        if (panel == null)
        {
            Debug.LogWarning("Attempting to create Item Container before creating proper Item Panel.\nCreating an item panel now before proceeding with Item Container");
            CreateItemPanel();
        }

        // Object setup
        Type layoutType = isVertical ? typeof(VerticalLayoutGroup) : typeof(HorizontalLayoutGroup);
        itemContainer = new GameObject("Item Container", typeof(ContentSizeFitter), layoutType /*typeof(VerticalLayoutGroup)*/ /*DONT FORGET TO UPDATE THIS*/).GetComponent<RectTransform>();
        itemContainer.parent = panel.transform;

        // UI position/alignment setup
        itemContainer.anchorMin = new Vector2(0, 1);
        itemContainer.anchorMax = new Vector2(0, 1);
        itemContainer.pivot = new Vector2(0, 1);
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

            layout.childControlWidth = isHorizontal;
            layout.childControlHeight = isVertical;

            layout.childScaleWidth = !isHorizontal;
            layout.childScaleHeight = !isVertical;
        }
        else
        {
            HorizontalLayoutGroup layout = itemContainer.GetComponent<HorizontalLayoutGroup>();

            layout.childAlignment = TextAnchor.UpperLeft;

            layout.childControlWidth = isHorizontal;
            layout.childControlHeight = isVertical;

            layout.childScaleWidth = !isHorizontal;
            layout.childScaleHeight = !isVertical;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //inventoryPanel.gameObject.SetActive(true);'
        //OpenInventory();
        isOpen = true;
        AnimateContainer();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //inventoryPanel.gameObject.SetActive(false);
        //CloseInventory();
        isOpen = false;
        AnimateContainer();
    }

    private void AnimateContainer()
    {
        if (coroutineAnimation != null)
            StopCoroutine(coroutineAnimation);

        coroutineAnimation = StartCoroutine(AnimateItems());
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

        float startPos = isVertical ? panel.rect.height : panel.rect.width;
        float elapsed = 0f;
        float animationDuration = 0.25f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float time = Mathf.Clamp01(elapsed / animationDuration);

            // smooth ease animation
            float smoothT = 1f - Mathf.Pow(1f - time, 3f);

            float newPos = Mathf.Lerp(startPos, targetPos, smoothT);

            panel.SetSizeWithCurrentAnchors(expandDirection, newPos);

            yield return null;
        }

        panel.SetSizeWithCurrentAnchors(expandDirection, targetPos);

        coroutineAnimation = null;
    }

    public void RefreshContainer()
    {
        // Remove old items
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (DropdownItem item in items)
        {
            GameObject newItem = Instantiate(itemPrefab, itemContainer);

            TMP_Text text = newItem.GetComponentInChildren<TMP_Text>();

            Image image = newItem.GetComponentInChildren<Image>();

            if (text != null)
                text.text = item.text;

            if (image != null)
                image = item.image;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(itemContainer);
    }

    public void AddItems(params DropdownItem[] items)
    {
        this.items.AddRange(items);
        RefreshContainer();
    }

    public void RemoveItem(DropdownItem item)
    {
        items.Remove(item);
        RefreshContainer();
    }

    public void RemoveAtIndex(int index)
    {
        items.RemoveAt(index);
        RefreshContainer();
    }

    public void SetItems(DropdownItem[] items)
    {
        this.items = items.ToList();
        RefreshContainer();
    }
    public void SetItems(List<DropdownItem> items)
    {
        this.items = items;
        RefreshContainer();
    }
}


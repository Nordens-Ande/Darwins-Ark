
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

    private List<string> inventoryItems = new List<string>(); //temporary list to test if functionality works

    [SerializeField, ReadOnly] private bool isOpen = false;

    private Coroutine animation;
    private float targetHeight;

    public void Start()
    {
        inventoryItems.Add("Test");
        inventoryItems.Add("Long Test");
        inventoryItems.Add("Longer Test");

        RefreshInventory();
        inventoryPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0f);
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

        targetHeight = isOpen ? LayoutUtility.GetPreferredHeight(itemContainer) : 0f;

        float startHeight = inventoryPanel.rect.height;
        float elapsed = 0f;
        float animationDuration = 0.25f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float time = Mathf.Clamp01(elapsed / animationDuration);

            // smooth ease animation
            float smoothT = 1f - Mathf.Pow(1f - time, 3f);

            float height = Mathf.Lerp(startHeight, targetHeight, smoothT);

            inventoryPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

            yield return null;
        }

        inventoryPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);

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

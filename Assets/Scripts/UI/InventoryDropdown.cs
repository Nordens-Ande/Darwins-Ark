
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

    public void Start()
    {
        inventoryItems.Add("Test");
        inventoryItems.Add("Long Test");
        inventoryItems.Add("Longer Test");

        inventoryPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0f);
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        //inventoryPanel.gameObject.SetActive(true);'
        OpenInventory();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //inventoryPanel.gameObject.SetActive(false);
        CloseInventory();
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

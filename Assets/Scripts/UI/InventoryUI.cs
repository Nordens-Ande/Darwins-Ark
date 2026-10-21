using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform gridParent;
    [SerializeField] private GameObject slotPrefab;

    private Inventory inventory;

    private List<InventorySlotUI> slotUIs =
        new List<InventorySlotUI>();

    private void Start()
    {
        inventory = InventoryManager.Instance.playerInventory;

        CreateSlots();
        RefreshInventory();

        inventory.OnInventoryChanged += RefreshInventory;
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= RefreshInventory;
        }
    }

    private void CreateSlots()
    {
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        slotUIs.Clear();

        for (int i = 0; i < inventory.MaxSlots; i++)
        {
            GameObject newSlot =
                Instantiate(slotPrefab, gridParent);

            InventorySlotUI slotUI =
                newSlot.GetComponent<InventorySlotUI>();

            slotUIs.Add(slotUI);
        }
    }

    public void RefreshInventory()
    {
        for (int i = 0; i < slotUIs.Count; i++)
        {
            slotUIs[i].SetSlot(
                inventory.Slots[i]
            );
        }
    }
}

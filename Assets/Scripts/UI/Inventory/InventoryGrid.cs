
using UnityEngine;

public class InventoryGrid : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform gridParent;

    [SerializeField] private int slotCount = 15;

    private void Start()
    {
        CreateSlots();
    }

    private void CreateSlots()
    {
        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, gridParent);
        }
    }
}

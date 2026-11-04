using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text amountText;

    public void SetSlot(InventorySlot slot)
    {
        if (slot == null || slot.IsEmpty)
        {
            itemIcon.enabled = false;
            amountText.text = "";
            return;
        }

        itemIcon.enabled = true;

        itemIcon.sprite = slot.seed.Icon;
    }
}

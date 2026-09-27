using UnityEngine;
using TMPro;

public class CurrencyUI : MonoBehaviour
{
    [SerializeField] private TMP_Text deityText;
    [SerializeField] private TMP_Text dnaText;

    private void Update()
    {
        if (CurrencyManager.Instance == null)
            return;

        deityText.text = "Deity points: " + CurrencyManager.Instance.DeityPoints;
        dnaText.text = "DNA points: " + CurrencyManager.Instance.DnaPoints;
    }
}

using TMPro;
using UnityEngine;

public class PopupScript : MonoBehaviour
{
    [SerializeField] private TextMeshPro titleTMP;
    [SerializeField] private TextMeshPro happinessTMP;
    [SerializeField] private TextMeshPro hungerTMP;
    [SerializeField] private TextMeshPro tiredTMP;
    [SerializeField] private TextMeshPro matingTMP;

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(Camera.main.transform.position);
        transform.rotation = Camera.main.transform.rotation;
        
    }
    public void SetPopupValues(AnimalGUIInfo info) 
    {
        titleTMP.text += "Buffalo";
        happinessTMP.text += info.Happiness.ToString();
        hungerTMP.text += info.Hunger.ToString();
        tiredTMP.text += info.Tired.ToString();
        matingTMP.text += info.MatingSeason.ToString();
    }
}

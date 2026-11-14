using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupScript : MonoBehaviour
{
    //[SerializeField] private TextMeshPro titleTMP;
    //[SerializeField] private TextMeshPro happinessTMP;
    //[SerializeField] private TextMeshPro hungerTMP;
    //[SerializeField] private TextMeshPro tiredTMP;
    //[SerializeField] private TextMeshPro matingTMP;
    SoundManager soundManager = SoundManager.Instance;

    [SerializeField] private TextMeshProUGUI titleTMP;
    [SerializeField] private TextMeshProUGUI happinessTMP;
    [SerializeField] private TextMeshProUGUI hungerTMP;
    [SerializeField] private TextMeshProUGUI tiredTMP;
    [SerializeField] private TextMeshProUGUI matingTMP;

    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Transform target;
    [SerializeField]private Button mutateButton;

    [SerializeField] private Vector3 offset = new Vector3(3f,3f,0);

    public Transform Target 
    { 
        get => target;
        set => target = value;
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null || rectTransform == null) return;
        rectTransform.position = Camera.main.WorldToScreenPoint(target.position + offset);
        //transform.rotation = Camera.main.transform.rotation;

    }

    public void AddAnimalToButton(GameObject obj) 
    {
        AnimalAI animal = obj.GetComponent<AnimalAI>();
        mutateButton.onClick.AddListener(animal.InvokeMutation);
        mutateButton.onClick.AddListener(ButtonClickSound);

    }

    void ButtonClickSound() 
    {
        soundManager.PlaySoundEffectAtVec3(transform.position, soundManager.GetAudioClipFromIndex(AudioType.uiPopupButtonClikc), 0, 100, 0.1f);
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

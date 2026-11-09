using Unity.VisualScripting;
using UnityEngine;

public class Popup_customButton : MonoBehaviour
{
    SoundManager soundManager = SoundManager.Instance;
    public void Onclick() 
    { 
        Transform root = this.transform.root;
        AnimalAI animal = root.GetComponent<AnimalAI>();
        soundManager.PlaySoundEffectAtVec3(root.position, soundManager.GetAudioClipFromIndex(AudioType.uiPopupButtonClikc), 0,100,0.1f);
        if (animal != null) 
        {
            animal.HasMutated = true;
            //MutationManager.Instance.MutateAnimal_RandomMutation(animal);
        }
    }
}

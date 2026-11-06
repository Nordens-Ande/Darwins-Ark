using UnityEngine;

public class Popup_customButton : MonoBehaviour
{
    public void Onclick() 
    { 
        Transform root = this.transform.root;
        AnimalAI animal = root.GetComponent<AnimalAI>();
        if(animal != null) 
        { 
            MutationManager.Instance.MutateAnimal_RandomMutation(animal);
        }
    }
}

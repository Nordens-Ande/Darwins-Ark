using UnityEngine;

public class Poop : MonoBehaviour
{
    Seed seed;

    //Method that can be called in the UI for picking up random seed and adding it to the inventory
    public void PickUpPoop() 
    {
        seed = SeedManager.Instance.GetRandomSeed();
        if (seed != null) 
        {
            seed.AddToInventory(Random.Range(1,3));
        }
        
    }
}

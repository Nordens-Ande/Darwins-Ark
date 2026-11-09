using System.Collections;
using UnityEngine;

public class Poop : MonoBehaviour
{
    Seed seed;
    private bool shouldMutate;
    MutationManager mutationManager = MutationManager.Instance;

    public bool ShouldMutate 
    { 
        get => shouldMutate; 
        set => shouldMutate = value;
    }

    private void Start()
    {
        PickUpPoop();
    }

    //Method that can be called in the UI for picking up random seed and adding it to the inventory
    public void PickUpPoop() 
    {
        seed = SeedManager.Instance.GetRandomSeed();
        if (seed != null) 
        {
            if (shouldMutate) 
            {
                mutationManager.MutatePlant_Random(seed);
            }
            else 
            {
                seed.AddToInventory(Random.Range(1, 3));
            }
        }

        StartCoroutine(PoopDesolve());
    }

    IEnumerator PoopDesolve() 
    { 
        yield return new WaitForSeconds(10);
        Destroy(gameObject);
    }
}

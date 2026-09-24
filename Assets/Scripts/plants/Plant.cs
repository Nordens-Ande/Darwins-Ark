using UnityEngine;


/// <summary>
/// This script is what every plant is, it holds thier atributes stats etc
/// </summary>


public class Plant : MonoBehaviour
{

    //every plant has this:
    [SerializeField] float growthSpeed = 1.0f;
    [SerializeField] GrowthStage currentStage = GrowthStage.seed;
    private GameObject currentModelInstance; //this is the model currently used for the plant


    //this is the models for the different stages of the plant
    [SerializeField] GameObject seedPrefab; 
    [SerializeField] GameObject sproutPrefab;
    [SerializeField] GameObject growthlingPrefab;
    [SerializeField] GameObject grownPrefab;

    
    public enum GrowthStage
    {
        seed,
        sprout,
        growthling,
        grown,
    }



    void Start()
    {
        updateVisulas();
    }


    //unity calls this everytime something is changed in the inspector
    private void OnValidate()
    {
        updateVisulas();
    }



    void updateVisulas()
    {
        //Clear out the previous model instance if it exists
        if (currentModelInstance != null)
        {
            Destroy(currentModelInstance);
        }

        //Figure out which prefab to instantiate
        GameObject prefabToSpawn = null;

        switch (currentStage)
        {
            case GrowthStage.seed:
                prefabToSpawn = seedPrefab;
                break;
            case GrowthStage.sprout:
                prefabToSpawn = sproutPrefab;
                break;
            case GrowthStage.growthling:
                prefabToSpawn = growthlingPrefab;
                break;
            case GrowthStage.grown:
                prefabToSpawn = grownPrefab;
                break;
        }

        //Spawn the new model directly as a child of this object
        if (prefabToSpawn != null)
        {
            currentModelInstance = Instantiate(prefabToSpawn, transform.position, transform.rotation, transform);
        }
    }
}

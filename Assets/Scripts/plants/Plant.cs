using UnityEngine;
using Assets.Scripts.Environment;


/// <summary>
/// This script is what every plant is, it holds thier atributes stats etc
/// </summary>


public class Plant : MonoBehaviour
{

    //every plant has this:
    [Header("Plant Stats")]
    [SerializeField] float growthSpeed = 1.0f;
    [SerializeField] float health = 100.0f;
    [SerializeField, ReadOnly] float timeSinceLastGrowth = 0.0f;
    [SerializeField, ReadOnly] bool canBeEaten = false;
    [SerializeField, ReadOnly] Tile currentTile; //The tile that the plant is currently on
    [Space]

    [Header("Plant Growth Stage")]
    [SerializeField] GrowthStage currentStage = GrowthStage.seed;
    [Space]

    //this is the models for the different stages of the plant
    [Header("Plant Models")]
    [SerializeField] GameObject seedPrefab; 
    [SerializeField] GameObject sproutPrefab;
    [SerializeField] GameObject growthlingPrefab;
    [SerializeField] GameObject grownPrefab;
    [Space]


    private GameObject currentModelInstance; //this is the model currently used for the plant
    public Tile CurrentTile => currentTile; //this is the tile that the plant is currently on

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

    private void Update()
    {
        //Update the time since last growth
        timeSinceLastGrowth += Time.deltaTime;

    }


    //unity calls this everytime something is changed in the inspector
#if UNITY_EDITOR
    private void OnValidate()
    {
        //we only want to update the prefab if the game is not running and the object is in the scene
        if (gameObject.scene.rootCount == 0)
        {
            return;
        }

        //This makes sure that the updateVisuals function is called after the inspector
        //has finished updating the serialized fields. So it does not create a new model
        //for every change in the inspector, but only after all changes have been made.
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null)
            {
                updateVisulas();
            }
        };
    }
#endif


    void ResetGrowthTimer()
    {
        timeSinceLastGrowth = 0.0f;
    }


    void updateVisulas()
    {
        //Clear out the previous model instance if it exists
        clearAllChildren();
        

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
            //currentModelInstance = Instantiate(prefabToSpawn, transform.position, transform.rotation, transform);
            currentModelInstance = Instantiate(prefabToSpawn, transform.position, prefabToSpawn.transform.rotation, transform);
        }
    }

    private void clearAllChildren()
    {
        // dont run if the its just an asset
        if (gameObject.scene.rootCount == 0) return;

        foreach (Transform child in transform)
        {
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                //i editor mode we need to use DestroyImmediate instead of Destroy
                DestroyImmediate(child.gameObject);
            }

        }
    }

    private void OnDestroy()
    {
        //if the plant is destroyed, we need to clear the tile it was on
        if (currentTile != null)
        {
            currentTile.ClearPlant();
        }
    }







    ///////////////////// <summary>
    ///////////////////// This is public functions
    public void DamagePlant(float damageAmount)
    {
        health -= damageAmount;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void GrowPlant()
    {
        //Increase the growth stage of the plant
        if (currentStage < GrowthStage.grown)
        {
            currentStage++;
            ResetGrowthTimer();
            updateVisulas();
        }

        //if the plant is fully gorwn we can eat it
        if (currentStage == GrowthStage.grown)
        {
            canBeEaten = true;
        }

    }

    //this will destroy the plant
    public void EatPlant()
    {
        if (canBeEaten)
        {
            DamagePlant(health); 
        }
    }

    public void OccupyTile(Tile tile)
    {
        currentTile = tile;
        if (tile != null)
        {
            tile.SetPlant(this);
        }
    }








    ///////////////////// <summary>
    ///////////////////// This is where getters are loctaed
    public float GrowthSpeed 
    {
        get 
        { 
            return growthSpeed; 
        }
    }

    public float TimeSinceLastGrowth
    {
        get
        {
            return timeSinceLastGrowth;
        }
    }

    public bool CanBeEaten
    {
        get
        {
            //returns true if the plant can be eaten or if it is fully grown
            return canBeEaten || currentStage == GrowthStage.grown;
        }
    }







}

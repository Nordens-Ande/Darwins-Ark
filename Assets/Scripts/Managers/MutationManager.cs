using NUnit.Framework;
using UnityEngine;

using System.Collections.Generic;
using System.Collections;

public class MutationManager : SingletonManager<MutationManager>
{
    //public static MutationManager Instance = null;
    [SerializeField] private List<SO_AnimalMutation> mutationsList = new List<SO_AnimalMutation>();
    [SerializeField] private ParticleSystem mutationParticleSystem;
    SeedManager seedManager = SeedManager.Instance;

    public List<SO_AnimalMutation> MutationList 
    {
        get => mutationsList;
        set => mutationsList = value;
    }

    //private void Awake()
    //{
    //    if (MutationManager.instance == null)
    //    {
    //        MutationManager.instance = this;
    //    }
    //}

    SO_AnimalMutation RandomMutation() 
    {
        SO_AnimalMutation mutation = mutationsList[Random.Range(0, mutationsList.Count)];
        return mutation;
    }
    public void MutateAnimal_SetMutation(AnimalAI thisAnimal, SO_AnimalMutation thisMutation)
    {
        if (thisAnimal.IsMutated) { return; }
        if (mutationsList.Count != 0)
        {
            SO_AnimalMutation mutation = thisMutation;
            Renderer animalRenderer = thisAnimal.GetComponent<Renderer>();
            if (animalRenderer != null)
            {
                animalRenderer.material.color = mutation.MaterialColor;
            }
            thisAnimal.DMG += mutation.Damage;
            thisAnimal.Health += mutation.Health;
            thisAnimal.WalkSpeed += mutation.WalkSpeed;
            thisAnimal.RunSpeed += mutation.RunSpeed;
            thisAnimal.transform.localScale = new Vector3
                (thisAnimal.transform.localScale.x + mutation.Localsize,
                 thisAnimal.transform.localScale.y + mutation.Localsize,
                 thisAnimal.transform.localScale.z + mutation.Localsize);

            //thisAnimal.HasMutated = true;
        }

        //Visual indicator that mutation happend
        if (mutationParticleSystem != null) 
        {
            Vector3 spawnPoint = new Vector3(thisAnimal.transform.position.x, thisAnimal.transform.position.y + thisAnimal.transform.localPosition.y, thisAnimal.transform.position.z);
            ParticleSystem obj = Instantiate(mutationParticleSystem, thisAnimal.transform.position, Quaternion.identity);
            obj.Play();
            StartCoroutine(DestroyParticleSystem(obj));

        }
    }

    //Mutates the seed and adds it to inventory
    public void MutatePlant_Random(Seed seed) 
    {
        Seed mutatedSeed = Instantiate(seed);
        Plant mutatedPlant = AssignNewPlantMutation(seed.plantPrefab);
        mutatedSeed = AssingNewSeedMutation(mutatedSeed, mutatedPlant);

        if (mutatedSeed == null) 
        {
            Debug.Log("Problem with mutatePlant_Random");
            seed.AddToInventory(Random.Range(0, 3));
        }
        else 
        { 
            mutatedSeed.AddToInventory(Random.Range(0, 3));
        }
    }

    //Mutates the scriptableobject so the inventory will show relevant data
    Seed AssingNewSeedMutation(Seed seed, Plant plant) 
    { 
        Seed newSeed = new Seed();
        newSeed.plantPrefab = plant;
        return newSeed;

        //Need values to be get and set for me to be able to change them here, rework the seed scriptable object
    }

    //Mutates the plant prefab so the visual effects and logic will be visualised
    Plant AssignNewPlantMutation(Plant plant) 
    {
        Plant newplant = Instantiate(plant);
        Renderer renderer = newplant.GetComponent<Renderer>();
        if (newplant == null || renderer == null)
        {
            return plant;
        }
        //TODO i also need the different prefab stages to be able to change its color and visuals. They need to be gettable

        //plant.GrowthSpeed = 2;
        //plant.PlantSmellRadiusValue = 2;
        //renderer.material.color = Color.white;
        //plant.Health += 10;
        //plant.IsCloseToWaterRange = 10;
        return newplant;
    }

    IEnumerator DestroyParticleSystem(ParticleSystem particle) 
    { 
        while(particle.isPlaying || particle.IsAlive()) 
        { 
            yield return new WaitForSeconds(particle.totalTime);
        }
        Destroy(particle.gameObject);
    }

    public void MutateAnimal_RandomMutation(AnimalAI thisAnimal) 
    {
        SO_AnimalMutation mutation = RandomMutation();
        MutateAnimal_SetMutation(thisAnimal, mutation);
    }
}


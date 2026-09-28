using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MutationManager : MonoBehaviour
{
    public static MutationManager instance = null;
    [SerializeField] private List<SO_AnimalMutation> mutationsList = new List<SO_AnimalMutation>();
    [SerializeField] private ParticleSystem mutationParticleSystem;

    public List<SO_AnimalMutation> MutationList 
    {
        get { return mutationsList; }
        set { mutationsList = value; }
    }
    
    private void Awake()
    {
        if(MutationManager.instance == null) 
        {
            MutationManager.instance = this;
        }
    }

    SO_AnimalMutation RandomMutation() 
    {
        SO_AnimalMutation mutation = mutationsList[Random.Range(0, mutationsList.Count)];
        return mutation;
    }
    public void MutateAnimal_SetMutation(AnimalAI thisAnimal, SO_AnimalMutation thisMutation)
    {
        if (thisAnimal.HasMutated) { return; }
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

            thisAnimal.HasMutated = true;
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


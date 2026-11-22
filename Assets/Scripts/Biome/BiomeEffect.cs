using Assets.Scripts.Environment;
using System;
using UnityEngine;

public class BiomeEffect 
{
    //Values that can be returned in relation to which biome and animalelementtype
    private float veryNegativeValue = -1;
    private float negativeValue = -0.5f; 
    private float neutralValue = 0;
    private float positiveValue = 0.5f;
    private float veryPositiveValue = 1;

    //Gets the charge that should be applied for the biome for the entity. Should work by entering both plant or animal
    public float GetValueChange(BiomeType activeBiome, BiomeType entityElementType) 
    {
        BiomeValueEffect effect = ValueCharge(activeBiome, entityElementType);
        float charge = GetCharge(effect);
        return charge;
        
    }

    //Method to dictate which value charge (positve/negative etc) should be applied
    BiomeValueEffect ValueCharge(BiomeType activeBiome, BiomeType entityElementType) 
    {
        switch (activeBiome) 
        { 
            case BiomeType.Normal:
                return BiomeValueEffect.neutral;
            case BiomeType.Autumn:
                return BiomeValueEffect.neutral;

            case BiomeType.fire:
                if(entityElementType == BiomeType.fire) 
                { 
                    return BiomeValueEffect.veryPositive;
                }
                else if(entityElementType == BiomeType.Infected) 
                {
                    return BiomeValueEffect.neutral;
                }
                else if(entityElementType == BiomeType.Ice) 
                {
                    return BiomeValueEffect.veryNegative;
                }
                else 
                {
                    return BiomeValueEffect.Negative;
                }
            case BiomeType.Ice:
                if (entityElementType == BiomeType.fire)
                {
                    return BiomeValueEffect.veryNegative;
                }
                else if (entityElementType == BiomeType.Infected)
                {
                    return BiomeValueEffect.positive;
                }
                else if (entityElementType == BiomeType.Ice)
                {
                    return BiomeValueEffect.veryPositive;
                }
                else
                {
                    return BiomeValueEffect.Negative;
                }

            case BiomeType.Infected:
                if (entityElementType == BiomeType.fire)
                {
                    return BiomeValueEffect.Negative;
                }
                else if (entityElementType == BiomeType.Infected)
                {
                    return BiomeValueEffect.veryPositive;
                }
                else if (entityElementType == BiomeType.Ice)
                {
                    return BiomeValueEffect.positive;
                }
                else
                {
                    return BiomeValueEffect.positive;
                }
            default: Console.WriteLine("Biomeeffect: Value charge error"); return BiomeValueEffect.neutral;
        }
    }

    //Gets the value connected to the charge, this to be applied to entities
    float GetCharge(BiomeValueEffect effect) 
    {
        switch (effect) 
        {
            case BiomeValueEffect.neutral:
                return neutralValue;
            case BiomeValueEffect.positive:
                return positiveValue;
            case BiomeValueEffect.veryPositive:
                return veryPositiveValue;
            case BiomeValueEffect.Negative:
                return negativeValue;
            case BiomeValueEffect.veryNegative:
                return veryNegativeValue;
            default: return neutralValue;
        }
    }

    public void ResetAnimalValue(float charge, AnimalAI animal) 
    {
        ApplyCharge_Animal(charge, animal);
        ApplyChange_Animal(animal);
    }

    //Maybe change this to be handled by plants/ animal themself
    void ApplyCharge_Animal(float charge, AnimalAI thisAnimal)   
    {
        thisAnimal.CurrentValues = thisAnimal.InitialValues;
        thisAnimal.CurrentValues.WalkSpeed += charge;
        thisAnimal.CurrentValues.RunSpeed += charge;
        thisAnimal.CurrentValues.Health += charge * 10;
        thisAnimal.CurrentValues.Damage += charge * 10;
        thisAnimal.CurrentValues.MutationRate += charge;
        thisAnimal.CurrentValues.HungerDeteration -= charge * 10;
    }

    //Think its a okay way, this dosent change the defult but applies the current changes
    void ApplyChange_Animal(AnimalAI thisAnimal) 
    {
        thisAnimal.WalkSpeed = thisAnimal.CurrentValues.WalkSpeed;
        thisAnimal.RunSpeed = thisAnimal.CurrentValues.RunSpeed;
        thisAnimal.Health = thisAnimal.CurrentValues.Health;
        thisAnimal.DMG = thisAnimal.CurrentValues.Damage;
        thisAnimal.MutationRatefloat = thisAnimal.CurrentValues.MutationRate;
        thisAnimal.HungerDeteration = thisAnimal.CurrentValues.HungerDeteration;
    }
}

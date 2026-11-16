using UnityEngine;


[CreateAssetMenu(fileName = "newPlantMutation", menuName = "Mutation/newPlantMutationData", order = 1)]
public class SO_PlantMutations : ScriptableObject
{
    [SerializeField] private Color color;
    [SerializeField] private float growthSpeed;
    [SerializeField] private float health;
    [SerializeField] private float plantSmellRadiusValue;
    [SerializeField] private float isCloseToWaterRange;

    public Color Color 
    { 
        get => color;
        set => color = value;
    }

    public float GrowthSpeed 
    { 
        get => growthSpeed;
        set => growthSpeed = value;
    }
    public float Health 
    { 
        get => health;
        set => health = value;
    }
    public float PlantSmellRadiusValue 
    { 
        get => plantSmellRadiusValue;
        set => plantSmellRadiusValue = value;
    }
    public float IsCloseToWaterRange 
    { 
        get => isCloseToWaterRange;
        set => isCloseToWaterRange = value;
    }
}

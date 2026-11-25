using UnityEngine;

public class PlantValues
{
    private float health;
    private float isCloseToWaterRange;
    private float growthSpeed;
    private float plantSmellRadius;

    public PlantValues(float health, float isclosetowater, float growthspeed, float plantsmellradius) 
    { 
        this.health = health;
        this.isCloseToWaterRange = isclosetowater;
        this.growthSpeed = growthspeed;
        this.plantSmellRadius = plantsmellradius;
    }

    public float Health 
    { 
        get => health; 
        set => health = value;
    }
    public float IsCloseToWaterRange 
    { 
        get => isCloseToWaterRange;
        set => isCloseToWaterRange = value;
    }
    public float growthspeed 
    { 
        get => growthSpeed;
        set => growthSpeed = value;
    }
    public float PlantSmellRadius 
    { 
        get => plantSmellRadius; 
        set => plantSmellRadius = value;
    }
}

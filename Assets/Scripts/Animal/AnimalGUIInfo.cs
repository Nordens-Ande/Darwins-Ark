using UnityEngine;

public class AnimalGUIInfo 
{
    //General
    private float walkSpeed;
    private float runSpeed;
    private float damage;
    private float health;
    private float hungerDeteration;
    private float mutationRate;

    //Utility points
    private float hunger;
    private float bossThreat;
    private float happiness;
    private float matingSeason;
    private float tired;

    public float WalkSpeed 
    { 
        get => walkSpeed;
        set => walkSpeed = value;
    } 
    public float RunSpeed 
    {
        get => runSpeed;
        set => runSpeed = value;
    }
    public float Damage
    { 
        get => damage;
        set => damage = value;
    }
    public float Health 
    { 
        get => health;
        set => health = value;
    }
    public float HungerDeteration
    { 
         get => hungerDeteration;
         set => hungerDeteration = value;
    }
    public float MutationRate 
    { 
        get => mutationRate; 
        set => mutationRate = value;
    }
    public float Hunger => hunger;
    public float Tired => tired;
    public float MatingSeason => matingSeason;

    public float Happiness => happiness;

    public AnimalGUIInfo
        (
        float walkspeed, 
        float runspeed,
        float dmg,
        float health,
        float hungerdeteration,
        float mutationrate,
        float hunger,
        float bosstreat,
        float happiness,
        float matingseason,
        float tired
        ) 
    { 
        this.walkSpeed = walkspeed;
        this.runSpeed = runspeed;
        this.damage = dmg;
        this.health = health;
        this.hungerDeteration = hungerdeteration;
        this.mutationRate = mutationrate;
        this.hunger = hunger;
        this.bossThreat = bosstreat;
        this.happiness = happiness;
        this.matingSeason = matingseason;
        this.tired = tired;
    }

}

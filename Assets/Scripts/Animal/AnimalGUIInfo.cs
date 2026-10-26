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

    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
    public float Damage => damage;
    public float Health => health;
    public float HungerDeteration => hungerDeteration;
    public float MutationRate => mutationRate;
    public float Hunger => hunger;
    public float Tired => tired;
    public float MatingSeason => matingSeason;

    public float Happiness => happiness;

    public AnimalGUIInfo(
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

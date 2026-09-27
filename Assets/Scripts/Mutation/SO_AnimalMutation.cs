using UnityEngine;


[CreateAssetMenu(fileName = "newMutation", menuName ="Mutation/newMutationData", order = 1)]
public class SO_AnimalMutation : ScriptableObject
{
    [SerializeField] private Color materialColor;
    [SerializeField] private float health;
    [SerializeField] private float DMG;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float hungerDeteration;
    [SerializeField] private float localsize;

    public Color MaterialColor 
    {
        get { return materialColor; }
    }

    public float Health 
    { 
        get {return health; }
    }
    public float Damage
    {
        get { return DMG; }
    }

    public float WalkSpeed
    {
        get { return walkSpeed; }
    }

    public float RunSpeed
    {
        get { return runSpeed; }
    }
    public float HungerDeteration
    {
        get { return hungerDeteration; }
    }
    public float Localsize
    {
        get { return localsize; }
    }
}

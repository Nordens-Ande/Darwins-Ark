using System;
using UnityEngine;

public class BirdAi : MonoBehaviour
{
    //Bird values
    private float health;
    private float dmg;
    private float speed;
    private float attackSpeed;

    //Updatelogic and attack targets
    private BirdState currentBirdState;
    private AnimalAI attackAnimal;
    private Plant attackPlant;
    private BirdTarget targetDemographic;

    //Public variables for FSM
    public float Speed => speed;
    public float AttackSpeed => attackSpeed;
    public BirdState CurrentBidState 
    {
        get => currentBirdState;
        set => currentBirdState = value; 
    }
    public AnimalAI AttackAnimal 
    { 
        get => attackAnimal;
        set => attackAnimal = value;
    }

    public Plant AttackPlant 
    {
        get => attackPlant;
        set => attackPlant = value;
    }
    public BirdTarget TargetDemographic 
    {
        get => targetDemographic;
        set => targetDemographic = value;
    }

    void Start()
    {
        health = 1;
        dmg = 10;
        speed = 1;
        attackSpeed = 3;
        SetRandomTarget();
    }

    void Update()
    {
        currentBirdState.Update(this);
    }

    //When player clicks on the bird this will be called
    public void BirdTakeDMG(float dmg) 
    { 
        health -= dmg;
        if (health < 0) 
        { 
            Destroy(gameObject);
        }
    }

    //Sets a random target from the beginning 
    //If there is no animal plants are choosen and vice versa
    //If there is no possible entities then the bird is destroyed and the happening should end
    void SetRandomTarget() 
    { 
        PlantManager plant = PlantManager.Instance;
        AnimalManager animal = AnimalManager.Instance;

        if (plant.Plants.Count > 0 && animal.Animals.Count > 0)
        {
            int i = Enum.GetNames(typeof(BirdTarget)).Length;
            int target = UnityEngine.Random.Range(1, i);
            targetDemographic = (BirdTarget)target;
            return;
        }
        else if(plant.Plants.Count > 0) 
        {
            targetDemographic = BirdTarget.plant;
        }
        else if(animal.Animals.Count > 0) 
        { 
            targetDemographic = BirdTarget.animal;
        }
        else 
        {
            Destroy(this.gameObject);
        }


    }
}

//State for the bird ai
public abstract class BirdState 
{ 
    public abstract void Enter(BirdAi ai);
    public abstract void Update(BirdAi ai);
    public abstract void Exit(BirdAi ai);
}

//Idle logic
public class BirdIdle : BirdState
{
    private float currentTime;
    private float maxTime;
    public override void Enter(BirdAi ai) 
    {
        currentTime = 0;
        maxTime = 8;
    }

    public override void Update(BirdAi ai) 
    { 
        currentTime += Time.deltaTime;
        if (currentTime > maxTime) 
        {
            Exit(ai);
        }
    }

    public override void Exit(BirdAi ai) 
    {
        AnimalManager animalManager = AnimalManager.Instance;
        if (animalManager != null) 
        { 
            foreach(AnimalAI a in animalManager.Animals) 
            {
                if(10 > Vector3.Distance(ai.transform.position, a.transform.position)) 
                {
                    ai.AttackAnimal = a;
                    ai.CurrentBidState = new BirdAttacking();
                    ai.CurrentBidState.Enter(ai);
                    return;
                }

            }
            ai.CurrentBidState = new BirdMove();
            ai.CurrentBidState.Enter(ai); 
            return;
        }
    }
}

//Move logic
public class BirdMove : BirdState
{
    private Vector3 targetPosition;
    private Vector3 orgin = new Vector3(0,20,0);
    public override void Enter(BirdAi ai)
    {
        targetPosition = new Vector3(UnityEngine.Random.Range(-20, 20), 20, UnityEngine.Random.Range(-20, 20));
    }

    public override void Update(BirdAi ai)
    {
        if (targetPosition != null)
        {
            ai.transform.position = Vector3.MoveTowards(ai.transform.position, targetPosition, ai.Speed * Time.deltaTime);
        }
        if (targetPosition == null || Vector3.Distance(ai.transform.position, targetPosition) < 1) 
        {
            Exit(ai);
        }
    }

    public override void Exit(BirdAi ai)
    {
        ai.CurrentBidState = new BirdIdle();
        ai.CurrentBidState.Enter(ai);
    }
}

//Attack logic
public class BirdAttacking : BirdState
{
    private float attackTime = 10;
    private float currentAttackTime = 0;
    private BirdTarget target;
    private AnimalAI targetAnimal;
    private Plant targetPlant;
    public override void Enter(BirdAi ai)
    {
        target = ai.TargetDemographic;
        if (target == BirdTarget.animal && ai.AttackAnimal != null) 
        { 
            targetAnimal = ai.AttackAnimal; 
        }
        if(target == BirdTarget.plant && ai.AttackPlant != null) 
        {
            targetPlant = ai.AttackPlant;
        }
    }

    public override void Update(BirdAi ai)
    {
        currentAttackTime += Time.deltaTime;
        if (target == BirdTarget.animal)
        {
            if (targetAnimal != null)
            {
                ai.transform.position = Vector3.MoveTowards(ai.transform.position, targetAnimal.transform.position, ai.AttackSpeed * Time.deltaTime);
            }
            if (targetAnimal == null || currentAttackTime > attackTime)
            {
                Exit(ai);
                return;
            }
            if (Vector3.Distance(ai.transform.position, targetAnimal.transform.position) < 1)
            {
                targetAnimal.TakeDMG(10);
                Exit(ai);
                return;
            }
        }
        else if(target == BirdTarget.plant) 
        {
            if (targetPlant != null)
            {
                ai.transform.position = Vector3.MoveTowards(ai.transform.position, targetPlant.transform.position, ai.AttackSpeed * Time.deltaTime);
            }
            if (targetPlant == null || currentAttackTime > attackTime)
            {
                Exit(ai);
                return;
            }
            if (Vector3.Distance(ai.transform.position, targetPlant.transform.position) < 1)
            {
                targetPlant.DamagePlant(10);
                Exit(ai);
                return;
            }
        }
        
    }

    public override void Exit(BirdAi ai)
    {
        ai.CurrentBidState = new BirdRetret();
        ai.CurrentBidState.Enter(ai);
    }
}

//After attack fase
public class BirdRetret : BirdState
{
    private Vector3 targetResetPosition;
    public override void Enter(BirdAi ai)
    {
        targetResetPosition = new Vector3(ai.transform.position.x, 20, ai.transform.position.z);
    }

    public override void Update(BirdAi ai)
    {
        ai.transform.position = Vector3.MoveTowards(ai.transform.position, targetResetPosition, ai.Speed * Time.deltaTime);
        if(Vector3.Distance(ai.transform.position, targetResetPosition) < 1) 
        {
            Exit(ai);
        }
    }

    public override void Exit(BirdAi ai)
    {
        ai.CurrentBidState = new BirdIdle();
        ai.CurrentBidState.Enter(ai);
    }
}

//Enums for possible target, right now plants and animals
public enum BirdTarget 
{ 
    animal,
    plant
}
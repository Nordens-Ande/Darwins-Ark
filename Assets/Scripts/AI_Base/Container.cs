using UnityEngine;


//These classes are the possible values from the animal given normalised
//If Ai need to be used in other then AnimalAI then make baseclass for AI and assign animalAI as a subclass
//Then have the subclasses as parameter for the Container, UtilityBrain

//Write the logic here aswell, which value that have priority over which
public abstract class Container : MonoBehaviour
{
    public abstract float Evaluate(AnimalAI animal);
}

public class BossValueContainer : Container 
{
    public override float Evaluate(AnimalAI animal)
    {
        if(animal.GetHappiness() < 1) 
        {
            return 0;
        }
        else 
        {
            return animal.BossThreat;
        }
    }
}

public class TiredContainer : Container
{
    public override float Evaluate(AnimalAI animal)
    {
        if (animal.BossThreat == 1 || animal.GetHappiness() < 1)
        {
            return 0;
        }
        else
        {
            return animal.Tired / 100;
        }
    }
}

public class MatingContainer : Container
{
    public override float Evaluate(AnimalAI animal)
    {
        return animal.MatingSeason / 100;
    }
}

public class LeaveContainer : Container
{
    public override float Evaluate(AnimalAI animal)
    {
        float invese = 100 - animal.GetHappiness();
        return invese /100;
    }
}

public class HungryContainer : Container
{
    public override float Evaluate(AnimalAI animal)
    {
        return animal.Hunger / 100;
    }
}

public class walkAroundContainer : Container
{
    public override float Evaluate(AnimalAI animal)
    {
        return 0.5f;
    }
}


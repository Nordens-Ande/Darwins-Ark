using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;


//Acts as the decisionmaking process for the animal where its given a list of UtilityActions and weighs what value is highest and performs the
//coresponding action in regards to it
//If UtilityBrain is supposed to be used in other ai then just make all AI a subclass of one class and then evaluate that
public class UtilityBrain<T>
{
    private List<UtilityAction<T>> actionList;
    private T agent;

    public UtilityBrain(List<UtilityAction<T>> actionList, T agent)
    {
        this.actionList = actionList;
        this.agent = agent;
    }

    public void DecisionProcess() 
    {
        if (actionList.Count == 0) return;

        UtilityAction<T> bestActionOption = actionList[0];
        float bestActionScore = actionList[0].Container.Evaluate(agent);

        foreach (UtilityAction<T> action in actionList) 
        { 
            float score = action.Container.Evaluate(agent);
            if (score > bestActionScore) 
            { 
                bestActionOption = action;
                bestActionScore = score;
            }
        }
        bestActionOption.Execute();
    }
}

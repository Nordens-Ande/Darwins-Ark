using UnityEngine;

//Takes in container of the value and what type of action that will correspond to that value
public class UtilityAction : MonoBehaviour
{
    private Container container;
    private System.Action action;

    public UtilityAction(Container container, System.Action action) 
    { 
        this.container = container;
        this.action = action;
    }

    public Container Container { get { return container; }}
    public System.Action Action { get { return action; }}

    public void Execute() 
    { 
        action?.Invoke();
    }
}

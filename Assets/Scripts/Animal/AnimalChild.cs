using Unity.VisualScripting;
using UnityEngine;

public class AnimalChild : MonoBehaviour
{
    private float growthTimerMax = 150;
    private float currentTime = 0;

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        if( currentTime > growthTimerMax) 
        { 
            GrowUp();
        }
    }
    void GrowUp() 
    { 
        AnimalAI childAI = transform.GetComponent<AnimalAI>();
        childAI.enabled = true;
        transform.localScale *= 3;
        Destroy(this);
    }
}

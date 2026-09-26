using UnityEngine;
using UnityEngine.UI;

public class AnimalMoodIndicator : MonoBehaviour
{
    [SerializeField] private AnimalAI animalAI;
    [SerializeField] private Image moodIndicator;

    private void Update()
    {
        UpdateMoodColor();
    }

    private void UpdateMoodColor()
    {
        if (animalAI == null || moodIndicator == null)
            return;

        float happiness = animalAI.GetHappiness();

        if (happiness >= 70f)
        {
            moodIndicator.color = Color.green;
        }
        else if (happiness >= 40f)
        {
            moodIndicator.color = Color.yellow;
        }
        else
        {
            moodIndicator.color = Color.red;
        }
    }
}

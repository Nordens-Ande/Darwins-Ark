using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    [SerializeField] private int deityPoints = 100;
    [SerializeField] private int dnaPoints = 100;

    public int DeityPoints => deityPoints;
    public int DnaPoints => dnaPoints;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        else
            Destroy(gameObject);
    }

    public void AddDeityPoints(int amount)
    {
        deityPoints += amount;
    }
    public bool SpendDeityPoints(int amount)
    {
        if (deityPoints < amount)
        {
            Debug.Log("You don't enough Deity points.");
            return false;
        }
            

        deityPoints -= amount;
        return true;
    }

    public void AddDnaPoints(int amount)
    {
        dnaPoints += amount;
    }

    public bool SpendDnaPoints(int amount)
    {
        if (dnaPoints < amount)
        {
            Debug.Log("You don't enough DNA points.");
            return false;
        }
           

        dnaPoints -= amount;
        return true;
    }
 }

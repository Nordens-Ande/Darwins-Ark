using UnityEngine;

public class SeedPickup : MonoBehaviour
{
    [SerializeField] private Seed seed;
    [SerializeField] private int amount = 1;

    public void PickUp()
    {
        if (seed = null)
            return;

        bool added = seed.AddToInventory(amount);

        if (added)
        {
            Destroy(gameObject);
        }
    }

}

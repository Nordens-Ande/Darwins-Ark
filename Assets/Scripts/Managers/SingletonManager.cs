using UnityEngine;

public class SingletonManager<T> : MonoBehaviour where T : SingletonManager<T>
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null && Instance != (T)this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = (T)this;
    }

    protected virtual void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}

using UnityEngine;

public abstract class BossTypeManager : MonoBehaviour
{
    public abstract void Initialize(Boss boss);
    public abstract void OnTick();
}

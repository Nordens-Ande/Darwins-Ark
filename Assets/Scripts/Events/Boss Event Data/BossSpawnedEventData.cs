using UnityEngine;

public class BossSpawnedEventData
{
    private Boss boss;

    public Boss Boss => boss;

    public BossSpawnedEventData(Boss boss)
    {
        this.boss = boss;
    }
}

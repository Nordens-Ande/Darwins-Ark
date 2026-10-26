using UnityEngine;

public class BossDeathEventData
{
    private BossHealth boss;

    public BossHealth Boss => boss;
    public BossDeathEventData(BossHealth boss)
    {
        this.boss = boss;
    }
}

using Assets.Scripts.Environment;
using UnityEngine;

public class BossDeathEventData
{
    private BossHealth boss;
    private int deityPoints;

    public BossHealth Boss => boss;
    public int DeityPoints => deityPoints;

    public BossDeathEventData(BossHealth boss, int deityPoints)
    {
        this.boss = boss;
        this.deityPoints = deityPoints;
    }
}

using Assets.Scripts.Environment;
using System.Collections.Generic;
using UnityEngine;

public class BossTerraformState : BossAIState
{
    private BossTerraManager bossTerraManager;
    private TileManager tileManager;

    public override void Initialize(Boss boss)
    {
        this.boss = boss;
        bossTerraManager = gameObject.GetComponent<BossTerraManager>();
        container = new BossTerraformContainer();
        tileManager = TileManager.Instance;
    }

    public override bool OnEnterState()
    {
        CanBeInterupted = false;
        TaskCompleted = false;

        //rotate towards attack tile

        return true;
    }

    public override void Tick() // add timer
    {
        Tile tile = bossTerraManager.CurrentAttackTile;
        Chunk chunk = tileManager.GetChunk(tile.GridPosition);
        if(tile.GetHeight() > bossTerraManager.ConcreteHeight)
        {
            chunk.IncreaseTileHeight(tile.GridPosition, -bossTerraManager.HeightDelta);
            if(tile.GetHeight() < bossTerraManager.ConcreteHeight)
            {
                tile.Height = bossTerraManager.ConcreteHeight;
                TaskCompleted = true;
                CanBeInterupted = true;
            }
        }
        else if(tile.GetHeight() < bossTerraManager.ConcreteHeight)
        {
            chunk.IncreaseTileHeight(tile.GridPosition, bossTerraManager.HeightDelta);
            if (tile.GetHeight() > bossTerraManager.ConcreteHeight)
            {
                tile.Height = bossTerraManager.ConcreteHeight;
                TaskCompleted = true;
                CanBeInterupted = true;
            }
        }
        else
        {
            TaskCompleted = true;
            CanBeInterupted = true;
        }
    }

    public override void OnExitState()
    {

    }
}

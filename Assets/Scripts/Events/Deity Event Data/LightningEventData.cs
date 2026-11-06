using UnityEngine;
using Assets.Scripts.Environment;
using System.Collections.Generic;

public class LightningEventData
{
    private Tile tile;

    public Tile Tile => tile;

    public LightningEventData(Tile tile)
    {
        this.tile = tile;
    }
}

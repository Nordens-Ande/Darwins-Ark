using System;
using UnityEngine;
using Assets.Scripts.Environment;
using System.Collections.Generic;
using UnityEditor;
using static UnityEngine.InputManagerEntry;

public class WindDeityManager : SingletonManager<WindDeityManager>
{
    private CurrencyManager currencyManager;
    private TileManager tileManager;

    private List<Wind> activeWindInstances;

    private int baseCost;

    public event Action OnWindSpawned;
    
    [SerializeField] private float windWidth;
    [SerializeField] private float windHeight; //the height of a tile to fully block wind.
    [SerializeField] private float maxWindStrength; 
    [SerializeField] private float windOffset; //acts as a weight for wind strength change regarding height difference, wind accel and retardation (:

    [SerializeField] public bool triggerWind = false;

    private void Start()
    {
        currencyManager = CurrencyManager.Instance;
        tileManager = TileManager.Instance;

        activeWindInstances = new List<Wind>();

        baseCost = 10;

        windWidth = 20f;
        windHeight = 2;
        maxWindStrength = 10;
        windOffset = 0.5f;
        
    }

    public void TriggerWind(Tile anchorPoint, Vector3 direction)
    {
        if(anchorPoint == null) return;
        direction.y = 0;
        Vector2 vec2Dir = new Vector2(direction.x, direction.z);

        Wind wind = new Wind(FindWindStrips(anchorPoint, vec2Dir), vec2Dir, windHeight, maxWindStrength, windOffset);
        activeWindInstances.Add(wind);
    }

    Vector2Int TileToVector2Int(Tile tile)
    {
        return new Vector2Int((int)tile.Position.x, (int)tile.Position.z);
    }

    private List<Tile> FindTilesInWindZone(Tile anchorPoint, Vector2 direction)
    {
        List<Tile> tilesAffectedByWind = new List<Tile>();
        foreach(Chunk chunk in tileManager.chunks)
        {
            foreach(Tile tile in chunk.Tiles)
            {
                if (tile.Type == TileType.Ocean) continue;
                if (tile == anchorPoint) continue;

                Vector2 offset = TileToVector2Int(tile) - TileToVector2Int(anchorPoint);
                float distanceAlongWind = Vector3.Dot(offset, direction);
                Vector2 perpendicular = new Vector2(-direction.y, direction.x);

                float distanceFromWind = Math.Abs(Vector2.Dot(offset, perpendicular));

                if (distanceFromWind > windWidth * 0.5f) continue;
                
                tilesAffectedByWind.Add(tile);
            }
        }
        tilesAffectedByWind.Add(anchorPoint);
        return tilesAffectedByWind;
    }

    private Dictionary<int, List<Tile>> FindWindStrips(Tile anchorPoint, Vector2 direction)
    {
        //List<List<Tile>> windStrips = new List<List<Tile>>();
        Dictionary<int, List<Tile>> windStrips = new Dictionary<int, List<Tile>>();
        
        List<Tile> tilesAffectedByWind = FindTilesInWindZone(anchorPoint, direction);

        if(tilesAffectedByWind.Count <= 0) return null;

        (int, int) rangeX = tileManager.GetTilePositionRangeX();
        (int, int) rangeZ = tileManager.GetTilePositionRangeZ();

        if (Math.Abs(direction.x) > Math.Abs(direction.y)) 
        {
            int minZ = rangeZ.Item1;
            int maxZ = rangeZ.Item2;
            //z low to high
            //x increasing
            for (int i = rangeX.Item1; i < rangeX.Item2; i++)
            {
                int listIndex = 0;
                for (int j = minZ; j < maxZ; j++)
                {
                    Tile tile = tileManager.GetTile(i, j);
                    if (tilesAffectedByWind.Contains(tile))
                    {
                        if (!windStrips.ContainsKey(listIndex))
                        {
                            windStrips[listIndex] = new List<Tile>();
                        }

                        if (listIndex == 0) minZ = j - 1;

                        windStrips[listIndex].Add(tile);
                        listIndex++;
                    }
                    else
                    {
                        if (listIndex > 0) maxZ = j + 1;
                    }
                }
            }
        }
        else
        {
            int minX = rangeX.Item1;
            int maxX = rangeX.Item2;
            //x low to high
            //z increasing
            for (int i = rangeZ.Item1; i < rangeZ.Item2; i++)
            {
                int listIndex = 0;
                for (int j = minX; j < maxX; j++)
                {
                    Tile tile = tileManager.GetTile(j, i);
                    if (tilesAffectedByWind.Contains(tile))
                    {
                        if (!windStrips.ContainsKey(listIndex))
                        {
                            windStrips[listIndex] = new List<Tile>();
                        }

                        if (listIndex == 0) minX = j - 1;

                        windStrips[listIndex].Add(tile);
                        listIndex++;
                    }
                    else
                    {
                        if (listIndex > 0) maxX = j + 1;
                    }
                }
            }
        }
        return windStrips;
    }

    private void Update()
    {
        if(triggerWind == true)
        {
            TriggerWind(tileManager.GetTile(new Vector2(0, 0)), new Vector3(0.5f, 0, 1).normalized);
            //TriggerWind(tileManager.GetTile(new Vector2(0, 0)), new Vector3(1, 0, 0));
            triggerWind = false;
        }
        
        for(int i = activeWindInstances.Count - 1;  i >= 0; i--)
        {
            Wind wind = activeWindInstances[i];
            if (wind == null) continue;

            if (wind.WindComplete == true)
            {
                activeWindInstances.RemoveAt(i);
                continue;
            }

            wind.Update(Time.deltaTime);
        }
    }
}

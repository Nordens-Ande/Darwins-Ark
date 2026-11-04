using UnityEngine;
using Assets.Scripts.Environment;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;
using System.Runtime.CompilerServices;

public class Wind
{
    private bool windComplete = false;

    private Dictionary<int, List<Tile>> windStrips;

    private Vector2 direction;    

    private float maxWindHeight;
    private float maxWindStrength;
    private float windOffset;

    private Dictionary<Tile, float> windStrengthAtTile;
    
    private int currentIndexToUpdate;
    private const float timeBetweenTile = 0.05f;
    private float updateTimer = 0;

    public bool WindComplete => windComplete;

    public Wind(Dictionary<int, List<Tile>> windStrips, Vector2 direction, float maxWindHeight, float maxWindStrength, float windOffset)
    {
        this.windStrips = windStrips;
        this.direction = direction;
        this.maxWindHeight = maxWindHeight;
        this.maxWindStrength = maxWindStrength;
        this.windOffset = windOffset;

        windStrengthAtTile = new Dictionary<Tile, float>();
        CalculateWindStrengths();

        currentIndexToUpdate = 0;
    }

    private List<Tile> OrderList(List<Tile> list)
    {
        List<Tile> result = new List<Tile>();
        if (direction.x > direction.y)
        {
            if (direction.x > 0)
            {
                result = list.OrderBy((tile => tile.Position.x)).ToList();
            }
            else
            {
                result = list.OrderByDescending((tile => tile.Position.x)).ToList();
            }
        }
        else
        {
            if (direction.y > 0)
            {
                result = list.OrderBy((tile => tile.Position.z)).ToList();
            }
            else
            {
                result = list.OrderByDescending((tile => tile.Position.z)).ToList();
            }
        }
        //Debug.Log("wind list count" + result.Count);
        return result;
    }

    private void CalculateWindStrengths()
    {
        for(int i = 0; i < windStrips.Count; i++)
        {
            windStrips[i] = OrderList(windStrips[i]);
            TraverseList(windStrips[i]);
        }
    }

    private void TraverseList(List<Tile> list)
    {
        float previousHeight = float.MaxValue;
        float previousStrength = float.MaxValue;

        for(int i = 0; i < list.Count; i++)
        {
            Tile tile = list[i];
            if (tile == null) continue;

            if(previousStrength == 0) // wind is dead
            {
                windStrengthAtTile[tile] = 0;
                continue;
            }

            if(previousHeight == float.MaxValue) // first tile
            {
                windStrengthAtTile[tile] = maxWindStrength;
                previousHeight = tile.GetHeight();
                previousStrength = maxWindStrength;
                continue;
            }

            float currentHeight = tile.GetHeight();
            float heightDiff = currentHeight - previousHeight;

            if(heightDiff == 0) // no diff same as last, no need to update local variables
            {
                windStrengthAtTile[tile] = previousStrength;
            }

            if (heightDiff > maxWindHeight) // steep enough to stop wind totally
            {
                previousStrength = 0;
                windStrengthAtTile[tile] = 0;
                continue;
            }

            float windStrength;
            float heightRatio = Mathf.Clamp01(Mathf.Abs(heightDiff) / maxWindHeight);
            float windChange = heightRatio * maxWindStrength;

            if(heightDiff > 0) // moved up, decrease wind strength
            {
                windStrength = previousStrength - windChange;
                windStrength = Mathf.Max(0, windStrength);
            }
            else // moved down, accelerate
            {
                windStrength = previousStrength + windChange;
                windStrength = Mathf.Min(maxWindStrength, windStrength);
            }

            previousHeight = currentHeight;
            previousStrength = windStrength;
            windStrengthAtTile[tile] = windStrength;
        }
    }

    private void ApplyWindEffect(Tile tile)
    {
        tile.ApplyWindEffect(windStrengthAtTile[tile], direction, maxWindStrength);
    }

    public void Update(float dt)
    {
        if (windComplete) return;

        if(updateTimer < timeBetweenTile)
        {
            updateTimer += dt;
            return;
        }
        updateTimer = 0;

        int amountUpdated = 0;
        for(int i = 0; i < windStrips.Count; i++)
        {
            if (!windStrips.ContainsKey(i)) continue;
            
            if(windStrips[i].Count - 1 >= currentIndexToUpdate)
            {
                ApplyWindEffect(windStrips[i][currentIndexToUpdate]);
                amountUpdated++;
            }
        }
        //Debug.Log("amount updwted" + amountUpdated);
        if(amountUpdated == 0)
        {
            //Debug.Log("wind complete");
            windComplete = true;
            return;
        }
        currentIndexToUpdate++;
    }
}

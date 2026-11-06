using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Environment;
using System.Linq;
using Unity.VisualScripting;

public class Lightning
{
    private Material material;

    private Transform managerTransform;

    private Dictionary<float, List<Tile>> tileDistanceDictionary;
    private Tile tile;
    private List<Tile> affectedTiles;

    //big lightning
    private int bigLightningHeight = 70;
    private int bigSegments = 70;

    //small lightning
    private int smallLightningLength = 1;
    private int smallSegments = 5;

    private List<float> distances;
    private int currentIndexToUpdate = 0;
    private const float timeBetweenTile = 0.02f;
    private float updateTimer = 0;

    private bool lightningComplete = false;

    public Lightning(Material material, Transform transform, Tile tile, List<Tile> affectedTiles)
    {
        Debug.Log("lightning class created");
        tileDistanceDictionary = new Dictionary<float, List<Tile>>();
        distances = new List<float>();
        
        this.material = material;
        this.managerTransform = transform;
        this.tile = tile;
        this.affectedTiles = affectedTiles;

        tileDistanceDictionary[0f] = new List<Tile> { tile };
        PopulateDictionary();
        distances.Sort();
        Debug.Log("distances" + distances.Count);
    }

    public void PopulateDictionary()
    {
        foreach(Tile affectedTile in affectedTiles)
        {
            float distance = Vector3.Distance(affectedTile.Position, tile.Position);

            distance = Mathf.Round(distance * 10f) / 10f;

            if(!tileDistanceDictionary.ContainsKey(distance))
            {
                distances.Add(distance);
                tileDistanceDictionary[distance] = new List<Tile>();
            }

            tileDistanceDictionary[distance].Add(affectedTile);
        }
    }

    private LineRenderer CreateLightning(Tile tile)
    {
        float displacement = 1f;
        float width = 0.4f;
        Color color = Color.lightBlue;

        Vector3 tilePos = tile.Position;
        Vector3 lightningStartPos = tilePos + Vector3.up * bigLightningHeight;

        GameObject mainLightning = new GameObject("mainLightning");
        mainLightning.transform.SetParent(managerTransform, true);

        LineRenderer line = mainLightning.AddComponent<LineRenderer>();

        line.material = material;

        line.positionCount = bigSegments + 1;
        line.useWorldSpace = true;

        line.startWidth = width;
        line.endWidth = width * 0.5f;

        line.startColor = color;
        line.endColor = color;

        line.numCapVertices = 4;
        line.numCornerVertices = 4;

        for (int i = 0; i <= bigSegments; i++)
        {
            float t = (float)i / bigSegments;

            Vector3 point = Vector3.Lerp(lightningStartPos, tilePos, t);

            if (i != 0 && i != bigSegments)
            {
                point += new Vector3(Random.Range(-displacement, displacement), Random.Range(-displacement / 2, displacement / 2), Random.Range(-displacement, displacement));
            }

            line.SetPosition(i, point);
        }
        return line;
    }

    private LineRenderer CreateSmallLightning(Tile tile)
    {
        Vector3 tilePos = tile.Position;
        Vector3 randPos = tilePos + new Vector3(Random.Range(-0.5f, 0.5f), 0.1f, Random.Range(-0.5f, 0.5f));
        float displacement = 0.3f;
        float width = 0.1f;
        Color color = Color.lightBlue;

        GameObject smallLightning = new GameObject("smallLightning");
        smallLightning.transform.SetParent(managerTransform, true);

        LineRenderer line = smallLightning.AddComponent<LineRenderer>();

        line.material = material;

        line.positionCount = smallSegments + 1;
        line.useWorldSpace = true;

        line.startWidth = width;
        line.endWidth = width * 0.5f;

        line.startColor = color;
        line.endColor = color;

        line.numCapVertices = 4;
        line.numCornerVertices = 4;

        for (int i = 0; i <= smallSegments; i++)
        {
            float t = (float)i / smallSegments;

            Vector3 point = Vector3.Lerp(randPos, tilePos, t);

            if (i != 0 && i != smallSegments)
            {
                point += new Vector3(Random.Range(-displacement, displacement), Random.Range(0, displacement / 4), Random.Range(-displacement, displacement));
            }

            line.SetPosition(i, point);
        }
        return line;
    }

    private IEnumerator DestroyLine(LineRenderer line)
    {
        yield return new WaitForSeconds(timeBetweenTile * ((distances.Count + 1) * 0.5f));
        Object.Destroy(line.gameObject);
    }
    
    public List<Tile> UpdateInstance(float dt)
    {
        if (lightningComplete) return null;

        if (updateTimer < timeBetweenTile)
        {
            updateTimer += dt;
            return null;
        }
        updateTimer = 0;

        if(currentIndexToUpdate > distances.Count - 1)
        {
            lightningComplete = true;
            return null;
        }

        List<Tile> updatedTiles = new List<Tile>();

        if(currentIndexToUpdate == 0)
        {
            Tile tile = tileDistanceDictionary[0][0];
            updatedTiles.Add(tile);
            CoroutineRunner.instance.StartCoroutine(DestroyLine(CreateLightning(tile)));
        }
        else
        {
            foreach (Tile tile in tileDistanceDictionary[distances[currentIndexToUpdate - 1]])
            {
                updatedTiles.Add(tile);
                for (int i = 0; i < 4; i++)
                {
                    CoroutineRunner.instance.StartCoroutine(DestroyLine(CreateSmallLightning(tile)));
                } 
            }
        }

        currentIndexToUpdate++;
        return updatedTiles;
    }
}
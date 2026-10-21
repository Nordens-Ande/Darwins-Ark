using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Environment;
using Unity.VisualScripting;

public class BossEscapeContainer : Container<Boss>
{
    public override float Evaluate(Boss boss)
    {
        float distanceTile = 0f;
        float distanceAnimal = 1f;
        float health = 0f;

        //distance to closest target (plant, river tile or other tile that boss will attack)
        //health
        //distance to closest animal, hmm

        if (boss.ClosestPlantTilePath == null)
        {
            distanceTile = 0f;
        }
        else
        {
            Tile plantTile = boss.ClosestPlantTilePath[boss.ClosestPlantTilePath.Count - 1];
            if (plantTile == null)
            {
                distanceTile = 0f;
            }
            else
            {
                float distance = Vector3.Distance(boss.transform.position, plantTile.position);
                distanceTile = Mathf.Clamp01(distance / boss.SightDistanceToTiles);
            }
        }

        health = 1 - boss.HealthScript.Health / boss.HealthScript.MaxHealth;

        List<AnimalAI> animal = boss.FindClosestAnimals(1);
        if (animal.Count == 0)
        {
            distanceAnimal = 0f;
        }
        else
        {
            float distance = Vector3.Distance(boss.transform.position, animal[0].transform.position);
            distanceAnimal = Mathf.Clamp01(1 - distance / boss.SightDistanceToAnimals);
        }
        if (boss.DebugScores)
            Debug.Log("Boss escape score: " + Mathf.Clamp01(0.2f * distanceTile + 0.5f * health + 0.3f * distanceAnimal));

        return Mathf.Clamp01(0.2f * distanceTile + 0.3f * health + 0.5f * distanceAnimal);
    }
}

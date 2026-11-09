using UnityEngine;
using Assets.Scripts.Environment;
using System.Collections;

/// <summary>
/// This class manages the particle system related to wind for the plants
/// </summary>
public class PlantParticleSystem : MonoBehaviour
{
    private WindDeityManager windDeityManager;
    private TileManager tileManager;
    private Tile tile;

    private ParticleSystem partSystem;

    private float gravityScale = 0.2f;

    private float minEmissionRate = 0f;
    private float maxEmissionRate = 10f;

    private float maxWindStrength;

    private float minWindSpeed = 0.5f;
    private float maxWindSpeed = 4f;

    private float particleLifetime = 2f;
    private float particleSize = 0.1f;

    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.MainModule main;
    private ParticleSystem.VelocityOverLifetimeModule velocity;

    void Start()
    {
        windDeityManager = WindDeityManager.Instance;
        tileManager = TileManager.Instance;
        if(tileManager != null)
        {
            tile = tileManager.GetTile(transform.position); 
        }

        maxWindStrength = windDeityManager.MaxWindStrength;

        partSystem = gameObject.AddComponent<ParticleSystem>();
        SetupParticleSystem();
        //StartCoroutine(StartParticleSystem());
    }

    private void SetupParticleSystem()
    {
        if (partSystem == null)
        {
            Debug.LogError("ParticleSystem is null!");
            return;
        }

        ParticleSystemRenderer particleRenderer = partSystem.GetComponent<ParticleSystemRenderer>();

        Material particleMaterial = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        if (particleMaterial != null)
        {
            particleRenderer.material = particleMaterial;
        }
        else
        {
            Debug.Log("particle material is null");
        }

        main = partSystem.main;
        
        main.startColor = Color.darkGreen;
        main.startLifetime = particleLifetime;
        main.startSize = particleSize;
        main.startSpeed = 0f;
        main.gravityModifier = gravityScale;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        emission = partSystem.emission;
        emission.enabled = false;
        emission.rateOverTime = minEmissionRate;
        emission.SetBursts(new ParticleSystem.Burst[0]);

        velocity = partSystem.velocityOverLifetime;
        velocity.enabled = false;

        velocity.space = ParticleSystemSimulationSpace.World;

        velocity.x = 0f;
        velocity.y = 0f;
        velocity.z = 0f;

        emission.enabled = true;
        velocity.enabled = true;
        partSystem.Play();
    }

    //IEnumerator StartParticleSystem()
    //{
    //    yield return new WaitForSeconds(Random.Range(0f, 3f));
    //    emission.enabled = true;
    //    velocity.enabled = true;
    //    partSystem.Play();
    //}

    public void UpdatePhysicsAttributes()
    {
        if(tile == null) return;
        if (partSystem == null) return;

        float windStrength = tile.WindStrength;
        Vector3 windDirection = tile.WindDirection;

        float normalizedWind = Mathf.Clamp01(windStrength / maxWindStrength);

        float emissionRate = Mathf.Lerp(minEmissionRate, maxEmissionRate, normalizedWind);
        emission.rateOverTime = emissionRate;

        float windSpeed = Mathf.Lerp(minWindSpeed, maxWindSpeed, normalizedWind);

        Vector3 direction = windDirection.normalized;

        velocity.x = direction.x * windSpeed;
        velocity.z = direction.z * windSpeed;
    }

    void Update()
    {
        UpdatePhysicsAttributes();
    }
}

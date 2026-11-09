using UnityEngine;
using System.Collections.Generic;
using System.Resources;
using UnityEngine.Assemblies;
using System;
using System.Runtime.CompilerServices;
using UnityEngine.Android;
using System.Threading;
using Assets.Scripts.Environment;

public class RainDeityManager : SingletonManager<RainDeityManager>
{
    private GameObject rainParticleSystemHolder;
    private Vector3 particleSystemPos = new Vector3(0, 70, 0);

    private ParticleSystem partSystem;
    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.MainModule main;
    private ParticleSystem.VelocityOverLifetimeModule velocity;
    private ParticleSystem.ShapeModule shape;

    private float emissionRate = 400;
    private float gravityScale = 1;
    private float particleSizeX = 0.05f;
    private float particleSizeY = 0.5f;
    private float particleLifetime = 5;

    private CurrencyManager currencyManager;
    private TileManager tileManager;

    private int baseCost;

    private bool isRaining;
    private float rainDuration;
    private float rainTimer;

    public event Action OnRainStarted;
    public event Action OnRainStopped;

    [SerializeField] public bool triggerRain = false; // for testing

    public string GetCostString => baseCost.ToString(); //for ui?

    private void Start()
    {
        currencyManager = CurrencyManager.Instance;
        tileManager = TileManager.Instance;
        baseCost = 10;
        isRaining = false;
        rainDuration = 20;
        rainTimer = 0f;

        rainParticleSystemHolder = new GameObject("Rain Particle System");
        rainParticleSystemHolder.transform.SetParent(transform);
        rainParticleSystemHolder.transform.localPosition = particleSystemPos;

        partSystem = rainParticleSystemHolder.AddComponent<ParticleSystem>();
        SetupParticleSystem();
    }

    private void SetupParticleSystem()
    {
        if (partSystem == null)
        {
            Debug.LogError("ParticleSystem is null! in rainmanager");
            return;
        }

        ParticleSystemRenderer particleRenderer = partSystem.GetComponent<ParticleSystemRenderer>();

        Material particleMaterial = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        if (particleMaterial != null)
        {
            particleRenderer.material = particleMaterial;

            particleRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            particleRenderer.lengthScale = 1f;
            particleRenderer.velocityScale = 0f;
        }
        else
        {
            Debug.Log("particle material is null, in rainmanager");
        }

        main = partSystem.main;
        main.startColor = Color.blue;
        main.startLifetime = particleLifetime;
        main.maxParticles = 10000;

        main.startSize3D = true;
        main.startSizeX = particleSizeX;
        main.startSizeY = particleSizeY;
        main.startSizeZ = 0.01f;

        main.startSpeed = 0f;
        main.gravityModifier = gravityScale;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        emission = partSystem.emission;
        emission.enabled = false;
        emission.rateOverTime = 1000;
        emission.rateOverDistance = 0;
        emission.SetBursts(new ParticleSystem.Burst[0]);

        velocity = partSystem.velocityOverLifetime;
        velocity.enabled = false;

        velocity.space = ParticleSystemSimulationSpace.World;

        velocity.x = 0f;
        velocity.y = -gravityScale;
        velocity.z = 0f;

        shape = partSystem.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;

        if(tileManager != null)
        {
            shape.scale = new Vector3(tileManager.ChunkGridSize.x * tileManager.ChunkSize, 1, tileManager.ChunkGridSize.y * tileManager.ChunkSize);
        }
        else
        {
            shape.scale = new Vector3(60f, 1f, 60f);
        }

        velocity.enabled = true;
        partSystem.Play();
    }

    public void TriggerRain() // on button press
    {
        if (isRaining) return;
        
        //uncomment this later after testing
        //if (!currencyManager.SpendDeityPoints(baseCost)) return;


        OnRainStarted?.Invoke();

        emission.enabled = true;
        
        isRaining = true;
    }

    private void Update()
    {
        if (triggerRain)
        {
            TriggerRain();
            triggerRain = false;
        }
        if (!isRaining) return;

        if(rainTimer > rainDuration)
        {
            rainTimer = 0;
            emission.enabled = false;

            isRaining = false;

            OnRainStopped?.Invoke();
        }
        else
        {
            rainTimer += Time.deltaTime;
        }
    }
}

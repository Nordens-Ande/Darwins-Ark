using Assets.Scripts.Environment;
using System.Collections.Generic;
using UnityEngine;

public class Rain
{
    private GameObject rainParticleSystemHolder;

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

    private List<Tile> affectedTiles = new List<Tile>();

    private float rainDuration;
    private float rainTimer;

    private float rainRadius;

    private bool rainComplete = false;

    public bool RainComplete => rainComplete;

    public Rain(float radius, Tile tile, List<Tile> affectedTiles)
    {
        this.rainRadius = radius;
        this.affectedTiles = affectedTiles;

        rainDuration = 20;
        rainTimer = 0f;

        foreach (Tile affectedTile in affectedTiles)
        {
            affectedTile.RainStarted();
        }

        rainParticleSystemHolder = new GameObject("Rain Particle System");
        rainParticleSystemHolder.transform.position = new Vector3(tile.Position.x, 70, tile.Position.z);

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

        main.startSpeed = -gravityScale;
        main.gravityModifier = gravityScale;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        emission = partSystem.emission;
        emission.enabled = false;
        emission.rateOverTime = emissionRate;
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
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = rainRadius * 1.5f;
        shape.radiusThickness = 1f;
        shape.rotation = new Vector3(-90, 0, 0);
        shape.scale = new Vector3(1, 1, 1);
        
        velocity.enabled = true;
        emission.enabled = true;
        partSystem.Play();
    }

    private void StopRain()
    {
        emission.enabled = false;

        foreach (Tile affectedTile in affectedTiles)
        {
            affectedTile.RainStopped();
        }

        rainComplete = true;
    }

    public void UpdateInstance(float dt)
    {
        if (rainTimer > rainDuration)
        {
            StopRain();
        }
        else
        {
            rainTimer += dt;
        }
    }
}

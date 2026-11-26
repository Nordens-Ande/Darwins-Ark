using Assets.Scripts.Environment;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class VFXManager : SingletonManager<VFXManager>
{
    //Ligth effects
    [SerializeField] private Light originalLight;
    private Color originalLight_Color;
    private float originalLight_Intensity;
    private float originalLight_Multiplier;

    //Particle effects
    private Dictionary<VFXenum, ParticleSystem> particleDictonary = new Dictionary<VFXenum, ParticleSystem>();
    [SerializeField] private List<VFX_KeyAndParticle> contentList = new List<VFX_KeyAndParticle>();

    //Camera effects
    [SerializeField] private List<CamVFX> camEffectList = new List<CamVFX>();
    private Dictionary<BiomeType, ParticleSystem> camVfxDictonary = new Dictionary<BiomeType, ParticleSystem>();
    [SerializeField] private bool ScreenEffect = false;

    //List for particlesystems that should play indenfinently
    //All gameobjects that subscribes will have particleeffects play untill they unsubscribe
    private Dictionary<GameObject, ParticleSystem> subscriberList = new Dictionary<GameObject, ParticleSystem>();

    [Serializable]
    public struct VFX_KeyAndParticle 
    {
        [SerializeField] private VFXenum key;
        [SerializeField] private ParticleSystem value;

        public VFXenum Key => key;
        public ParticleSystem Value => value;
    }

    [Serializable]
    public struct CamVFX 
    {
        [SerializeField] private BiomeType key;
        [SerializeField] private ParticleSystem value;
        public BiomeType Key => key;
        public ParticleSystem Value => value;
    }

    //Debugging the shake function
    [SerializeField] private bool debugShake = false;
    [SerializeField] private float time = 0.15f;
    [SerializeField] private float power = 1f;

    ParticleSystem cameraParticleSystem = null;
    BiomeManager biomeManager;
    Camera cam;

    private void Start()
    {
        if (originalLight != null)
        {
            SetStartLightValues();
        }

        cam = Camera.main;
        biomeManager = BiomeManager.Instance;
        if (ScreenEffect)
        {
            biomeManager.OnBiomeChange += SetBiomeCameraVFX;
        }


        if (contentList != null)
        {
            foreach (var content in contentList)
            {
                particleDictonary.TryAdd(content.Key, content.Value);
            }
        }

        if (camEffectList != null) 
        {
            foreach (var camEffect in camEffectList) 
            {
                camVfxDictonary.TryAdd(camEffect.Key, camEffect.Value);
            }
        }
        if (ScreenEffect)
        {
            SetBiomeCameraVFX(BiomeType.Normal);
        }


    }

    void SetStartLightValues() 
    {
        originalLight_Color = originalLight.color;
        originalLight_Intensity = originalLight.intensity;
        originalLight_Multiplier = originalLight.bounceIntensity;
    }

    public ParticleSystem GetParticleSystem(VFXenum key) 
    { 
        particleDictonary.TryGetValue(key, out var particle);
        return particle;
    }

    //Method for getting, playing and when done destroying the particlesystem
    public void GetSetAndDestroyVFX(VFXenum key, Vector3 position) 
    { 
        particleDictonary.TryGetValue (key, out var particle);

        if(particle != null) 
        {
            Vector3 spawnPoint = position;
            ParticleSystem obj = Instantiate(particle, spawnPoint, Quaternion.identity);
            obj.Play();
            StartCoroutine(DestroyParticleSystem(obj));
        }
    }

    public void GetSetAndDestroyVFX(VFXenum key, Vector3 position, Vector3 rotation)
    {
        particleDictonary.TryGetValue(key, out var particle);

        if (particle != null)
        {
            Vector3 spawnPoint = position;
            ParticleSystem obj = Instantiate(particle, spawnPoint, Quaternion.Euler(rotation));
            obj.Play();
            StartCoroutine(DestroyParticleSystem(obj));
        }
    }

    IEnumerator DestroyParticleSystem(ParticleSystem particle)
    {
        while (particle.isPlaying || particle.IsAlive())
        {
            yield return new WaitForSeconds(particle.totalTime);
        }
        Destroy(particle.gameObject);
    }


    //New method for subscribing to the vfx manager to continously play a particlesystem without destroying it 
    //Has the subscriber pattern and makes sure that only vfx manager handles the particlesystems logic
    public void Subscribe_ContinousPlay(GameObject obj, ParticleSystem particleSystem) 
    {
        if (subscriberList.TryGetValue(obj, out var test) != false) return; //Makes sure the obj isnet subsrcibed
        if (particleSystem != null)
        {
            ParticleSystem particles = Instantiate(particleSystem, obj.transform.position, Quaternion.identity);
            subscriberList.TryAdd(obj, particles);
        }

        subscriberList.TryGetValue(obj, out var particle);
        particle.Play();
    }

    public void UnSubscribe_StopContiniousPlay(GameObject obj) 
    { 
        subscriberList.TryGetValue(obj, out var particleSystem);
        if (particleSystem != null) 
        { 
            particleSystem.Stop();
            subscriberList.Remove(obj);
            Destroy(particleSystem.gameObject);
        }
    }

    public void ShakeCamera(float time, float power) 
    {
        StartCoroutine(CamShake(time, power));
    }

    IEnumerator CamShake(float time, float power) 
    {
        float currentTime = 0;
        Camera cam = Camera.main;
        Vector3 orginTransform = cam.transform.localPosition;
        while (currentTime < time) 
        {
            float xOffset = UnityEngine.Random.Range(-0.5f, 0.5f);
            float yOffset = UnityEngine.Random.Range(-0.5f, 0.5f);
            cam.transform.localPosition += new Vector3(xOffset * power, yOffset * power, 0);
            currentTime += Time.deltaTime;
            yield return null;
        }
        cam.transform.localPosition = orginTransform;
    }

    public void SetBiomeCameraVFX(BiomeType type) 
    { 
        if(cam.transform.childCount != 0) 
        {
            Destroy(cam.transform.GetChild(0).gameObject);
        }

        ParticleSystem part = new ParticleSystem();
        if(camVfxDictonary.ContainsKey(type)) 
        { 
            part = camVfxDictonary[type];
            ParticleSystem sys = Instantiate(part, cam.transform);
            sys.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y * 3, cam.transform.position.z);
            
            //sys.transform.rotation = cam.transform.rotation;
            sys.transform.SetParent(cam.transform);
        }
        
    }

    private void Update()
    {
        if (debugShake) 
        {
            ShakeCamera(time, power);
            debugShake = false;
        }
    }
}

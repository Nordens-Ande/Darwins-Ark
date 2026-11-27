using Assets.Scripts.Environment;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class VFXManager : SingletonManager<VFXManager>
{
    [Header("Light effects")]
    //Ligth effects
    [SerializeField] private Light originalLight;
    private LightEffect originalLightValues;
    private Dictionary<VFXLightEnum, LightEffect> lightEffectDictornry = new Dictionary<VFXLightEnum, LightEffect>();
    [SerializeField] private List<VFX_KeyAndLightEffect> lightList = new List<VFX_KeyAndLightEffect>();

    [Header("ParticleEffects")]
    //Particle effects
    private Dictionary<VFXenum, ParticleSystem> particleDictonary = new Dictionary<VFXenum, ParticleSystem>();
    [SerializeField] private List<VFX_KeyAndParticle> contentList = new List<VFX_KeyAndParticle>();

    [Header("Cam effects")]
    //Camera effects
    [SerializeField] private List<CamVFX> camEffectList = new List<CamVFX>();
    private Dictionary<BiomeType, ParticleSystem> camVfxDictonary = new Dictionary<BiomeType, ParticleSystem>();
    [SerializeField] private bool ScreenEffect = false;

    //List for particlesystems that should play indenfinently
    //All gameobjects that subscribes will have particleeffects play untill they unsubscribe
    private Dictionary<GameObject, ParticleSystem> subscriberList = new Dictionary<GameObject, ParticleSystem>();

    [Header("CloudEffect values")]
    //CloudEffect
    [SerializeField] private GameObject cloudPartPrefab;
    [SerializeField] private int minPartAmount;
    [SerializeField] private int maxPartAmount;
    [SerializeField] private int MaxCloudNbr;
    private int activeCloudCount = 0;

    public int ActiveCloudCount 
    { 
        get => activeCloudCount; set => activeCloudCount = value;
    }

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

    [Serializable]
    public struct LightEffect 
    {
        [SerializeField] private Color lightColor;
        [SerializeField] private float lightIntensity;
        [SerializeField] private float lightMultiplier;

        public Color LightColor 
        {  
            get => lightColor; 
            set => lightColor = value;
        }
        public float LightIntensity 
        { 
            get => lightIntensity; 
            set => lightIntensity = value;
        }
        public float LightMultiplier 
        { 
            get => lightMultiplier; 
            set => lightMultiplier = value;
        }
    }

    [Serializable]
    public struct VFX_KeyAndLightEffect
    {
        [SerializeField] private VFXLightEnum key;
        [SerializeField] private LightEffect lightEffect; 
        public VFXLightEnum Key => key;
        public LightEffect LightEffect => lightEffect;
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

        if (contentList != null && contentList.Count != 0)
        {
            foreach (var content in contentList)
            {
                particleDictonary.TryAdd(content.Key, content.Value);
            }
        }

        if (camEffectList != null && camEffectList.Count != 0) 
        {
            foreach (var camEffect in camEffectList) 
            {
                camVfxDictonary.TryAdd(camEffect.Key, camEffect.Value);
            }
        }

        if(lightList != null && lightList.Count != 0) 
        { 
            foreach(var light in lightList) 
            {
                lightEffectDictornry.TryAdd(light.Key, light.LightEffect);
            }
        }

        if (ScreenEffect)
        {
            SetBiomeCameraVFX(BiomeType.Normal);
        }
    }
    void SetStartLightValues() 
    {
        originalLightValues.LightColor = originalLight.color;
        originalLightValues.LightIntensity = originalLight.intensity;
        originalLightValues.LightMultiplier = originalLight.bounceIntensity;
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

    //Light effects

    //Set to original lightvalues
    public void SetToOriginalLight() 
    {
        originalLight.color = originalLightValues.LightColor;
        originalLight.intensity = originalLightValues.LightIntensity;
        originalLight.bounceIntensity = originalLightValues.LightMultiplier;
    }

    //Set to new type of light
    public void SetNewLight(VFXLightEnum type) 
    {
        lightEffectDictornry.TryGetValue(type, out LightEffect choosen);
        originalLight.color = choosen.LightColor;
        originalLight.intensity= choosen.LightIntensity;
        originalLight.bounceIntensity= choosen.LightMultiplier;
    }

    //Clouds
    void InstantiateClouds() 
    {
        if(activeCloudCount < MaxCloudNbr) 
        {
            StartCoroutine(CreateCloud());
        }
        
    }

    IEnumerator CreateCloud() 
    {
        int part = UnityEngine.Random.Range(minPartAmount, maxPartAmount);
        List<GameObject> list = new List<GameObject>();
        for (int i = 0; i < part; i++)
        {
            list.Add(cloudPartPrefab);
        }
        Clouds cloud = new Clouds(list);
        activeCloudCount++;
        yield return null;
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

public class Clouds : MonoBehaviour
{
    VFXManager manager = VFXManager.Instance;
    List<GameObject> cloudParts;
    private int endXPosition = 50; //??? might be good
    private int offsetFromLight;  //Dont now the offset from the light
    public Clouds(List<GameObject> cloudParts)
    {
        this.cloudParts = cloudParts;
    }
    GameObject parent = new GameObject("Parent");

    private void Start()
    {
        int randomYValue = UnityEngine.Random.Range(-5, 5);
        foreach (GameObject cloudPart in cloudParts) 
        {
            cloudPart.transform.position = new Vector3(cloudPart.transform.position.x, randomYValue, offsetFromLight);
            cloudPart.transform.SetParent(parent.transform);
        }

    }

    private void Update()
    {
        foreach(GameObject cloudPart in cloudParts) 
        {
            Vector3 newVec = new Vector3(endXPosition, cloudPart.transform.position.y, cloudPart.transform.position.z);
            cloudPart.transform.position = Vector3.MoveTowards(cloudPart.transform.position, newVec, Time.deltaTime);
            if(Vector3.Distance(cloudPart.transform.position, newVec) < 1) 
            {
                Destroy(cloudPart);
            }
        }

        if (cloudParts.Count == 0) 
        {
            manager.ActiveCloudCount--;
            Destroy(parent);
            Destroy(this.gameObject);
        }
    }
}

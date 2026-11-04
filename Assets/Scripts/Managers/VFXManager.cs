using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager instance = null;

    private Dictionary<VFXenum, ParticleSystem> particleDictonary;
    [SerializeField] private List<VFX_KeyAndParticle> contentList = new List<VFX_KeyAndParticle>();

    //List for particlesystems that should play indenfinently
    //All gameobjects that subscribes will have particleeffects play untill they unsubscribe
    private Dictionary<GameObject, ParticleSystem> subscriberList = new Dictionary<GameObject, ParticleSystem>();

    //Error when i made the dictonary serializable, this is a work around
    [Serializable]
    public struct VFX_KeyAndParticle 
    {
        [SerializeField] private VFXenum key;
        [SerializeField] private ParticleSystem value;

        public VFXenum Key => key;
        public ParticleSystem Value => value;
    }

    private void Awake()
    {
        if (contentList != null) 
        {
            foreach (var content in contentList) 
            {
                particleDictonary.TryAdd(content.Key, content.Value);
            }
        }
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
        subscriberList.TryAdd(obj, particleSystem);
        subscriberList.TryGetValue(obj, out var particle);
        if(particle != null) 
        {
            particleSystem.Play();
        }
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
}

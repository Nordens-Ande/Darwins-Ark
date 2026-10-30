using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    static VFXManager instance = null;

    private Dictionary<VFXenum, ParticleSystem> particleDictonary;
    [SerializeField] private List<VFX_KeyAndParticle> contentList = new List<VFX_KeyAndParticle>();

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
}

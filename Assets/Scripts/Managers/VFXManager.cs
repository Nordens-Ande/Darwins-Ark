using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    [SerializeField] private Dictionary<VFXenum, ParticleSystem> particleDictonary = new Dictionary<VFXenum, ParticleSystem>();

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

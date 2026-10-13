using UnityEngine;

[CreateAssetMenu(fileName = "newSoundeffect", menuName = "Soundeffect/newSoundeffectData")]

public class SO_SoundEffect : ScriptableObject
{
    [SerializeField] private AudioType audioType;
    [SerializeField] private AudioClip clip;

    public AudioType AudioType => audioType;
    public AudioClip Clip => clip;
    
}

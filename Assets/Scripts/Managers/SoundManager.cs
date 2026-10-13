using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{

    public static SoundManager Instance = null;

    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioSource backgroundMusicPlayer; //Where the backgroundmusic will be played 

    //List that holds all the audio in form of scriptable objects created 
    [SerializeField] private List<SO_SoundEffect> soundEffectsList;

    //Dictonary to get soundclips with audiotype
    private Dictionary<AudioType, AudioClip> soundIndex = new Dictionary<AudioType, AudioClip>();

    //Dictonary for mixertype, used in Playsoundeffectscript
    [SerializeField] private Dictionary<MixerType, AudioMixerGroup> mixerIndex = new Dictionary<MixerType, AudioMixerGroup>();


    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
        }
        else 
        {
            Destroy(this);
        }

        //Populates the dictonary with the scripble objects, connecting the type to the clip
        foreach (SO_SoundEffect audio in soundEffectsList)
        {
            if (audio.Clip == null) continue;
            soundIndex.Add(audio.AudioType, audio.Clip);
        }
    }

    //Starts the backgroundmusic
    public void StartBackgroundMusic() 
    { 
        backgroundMusicPlayer.clip = backgroundMusic;
        backgroundMusicPlayer.Play();
    }

    //Stops the backgroundmusic
    public void StopBackgroundMusic() 
    {
        backgroundMusicPlayer.Stop();
    }

    //Changes the backgroudmusic
    public void SetBackgroundMusic(AudioClip clip) 
    {
        backgroundMusic = clip;
        backgroundMusicPlayer.clip = backgroundMusic;
    }

    //Useless??? But it does store all the sound logic in this manager
    public void PlaySoundeffect(AudioSource source, AudioClip clip) 
    { 
        source.PlayOneShot(clip);
    }

    //Will return the audioclip that has the audiotype as the key
    //Makes it more secure then relying on strings
    public AudioClip GetAudioClipFromIndex(AudioType type) 
    {
        if (soundIndex.ContainsKey(type))
        {
            return soundIndex[type];
        }
        else 
        {
            Debug.Log(type + " has no soundclip attached");
            return null; 
        }
    }

    //Gets the soundeffect of type and then plays from set audiosource
    public void PlaySoundeffectFromIndex(AudioSource source, AudioType type) 
    { 
        AudioClip clip = GetAudioClipFromIndex(type);
        source.PlayOneShot(clip);
    }

    //Made customized playclipatpoint but so the audiomixer and mindistance and maxdistance are included
    public void PlaySoundEffectAtVec3(Vector3 position, AudioClip clip, float minDistance, float maxDistance, MixerType mixerType) 
    {
        GameObject audioObj = new GameObject("Temp audio");
        audioObj.transform.position = position;
        AudioSource audioSource = audioObj.transform.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = mixerIndex[mixerType]; //Can be unstable, no check if it exists
        audioSource.clip = clip;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.loop = false;
        audioSource.playOnAwake = true;
        InstantiateSoundEffect(audioObj, clip);
    }

    //Creates and destroyes the temporery gameobject after the clip finished playing
    //OBS used in PlaySOundEffectAtVec3
    private void InstantiateSoundEffect(GameObject source, AudioClip clip) 
    { 
        Instantiate(source);
        AudioSource audio = source.GetComponent<AudioSource>();
        audio.PlayOneShot(clip);
        Destroy(source, clip.length);
    }

    //When eventsystem is implemented this method can subscribe to it 
    public void BossBackgroundMusic() 
    {
        backgroundMusicPlayer.Stop();
        backgroundMusic = soundIndex[AudioType.backgroundMusic_bossfight];
        backgroundMusicPlayer.clip = backgroundMusic;
        backgroundMusicPlayer.Play();
    }

    //Sets the backgroundmusic back to Idle
    public void IdleBackgroundMusic() 
    {
        backgroundMusicPlayer.Stop();
        backgroundMusic = soundIndex[AudioType.backgroundMusic_Idle];
        backgroundMusicPlayer.clip = backgroundMusic;
        backgroundMusicPlayer.Play();
    }
}

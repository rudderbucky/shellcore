using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioMixer masterMixer;
    public AudioMixerGroup sounds;
    public AudioMixerGroup music;
    public static AudioManager instance;
    public AudioSource playerSource;
    public AudioSource playerMusicSource;
    
    // change this ID to override sector music
    public static string musicOverrideID = null;
    private Dictionary<string, float> timePlayed;

    public void Initialize()
    {
        instance = this;
        timePlayed = new Dictionary<string, float>();
        musicOverrideID = null;
    }
    void Start() 
    {
        if(!masterMixer) return;
        ChangeMasterVolume(PlayerPrefs.GetFloat("MasterVolume", 0.5f));
        ChangeMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 1f));
        ChangeSoundEffectsVolume(PlayerPrefs.GetFloat("SFXVolume", 1f));
    }

    public void ChangeMasterVolume(float newVol) 
    {
        if(masterMixer)
            masterMixer.SetFloat("MasterVolume", Mathf.Log10(newVol) * 20);
        PlayerPrefs.SetFloat("MasterVolume", newVol);
    }

    public void ChangeMusicVolume(float newVol) 
    {
        if(masterMixer)
            masterMixer.SetFloat("MusicVolume", Mathf.Log10(newVol) * 20);
        PlayerPrefs.SetFloat("MusicVolume", newVol);
    }

    public void ChangeSoundEffectsVolume(float newVol) 
    {
        if(masterMixer)
            masterMixer.SetFloat("SFXVolume", Mathf.Log10(newVol) * 20);
        PlayerPrefs.SetFloat("SFXVolume", newVol);
    }

    public static void PlayClipByID(string ID, Vector3 pos) {
        if(instance.timePlayed.ContainsKey(ID) && instance.timePlayed[ID] == Time.time)
            return;

        var source = new GameObject().AddComponent<AudioSource>();
        source.transform.position = pos;
        source.name = "Audio One-Shot";
        source.outputAudioMixerGroup = instance.sounds;
        source.clip = ResourceManager.GetAsset<AudioClip>(ID);
        source.Play();
        if(!instance.timePlayed.ContainsKey(ID))
            instance.timePlayed.Add(ID, Time.time);
        else
            instance.timePlayed[ID] = Time.time;
        
        Destroy(source.gameObject, source.clip.length);
    }

    // Plays the clip directly on the player
    public static void PlayClipByID(string ID, bool clear=false) {
        

        if(instance.playerSource != null) {
            if(clear) instance.playerSource.Stop();
            if(ID != null) 
            {
                if(instance.timePlayed.ContainsKey(ID) && instance.timePlayed[ID] == Time.time)
                    return;

                var clip = ResourceManager.GetAsset<AudioClip>(ID);
                instance.playerSource.PlayOneShot(clip, 1F);
                if(!instance.timePlayed.ContainsKey(ID))
                    instance.timePlayed.Add(ID, Time.time);
                else
                    instance.timePlayed[ID] = Time.time;
            }
            // can pass null just to clear the sound buffer
        }
        // TODO: Add audio sources to places that need it
    }

    // Use for Soundtrack
    public static void OverrideMusicTemporarily(string ID)
    {
        var curClip = instance.playerMusicSource.clip;
        instance.playerMusicSource.Stop();
        var newClip = ResourceManager.GetAsset<AudioClip>(ID);
        instance.playerMusicSource.PlayOneShot(newClip);
        // TODO: Maybe continue the main music instead of restarting it but mute it somehow?
        instance.playerMusicSource.PlayDelayed(newClip.length + 5F);
    }
    
    public static void PlayMusic(string ID, bool loop=true)
    {
        if(instance.playerMusicSource != null)
        {
            instance.playerMusicSource.loop = loop;
            var clip = ResourceManager.GetAsset<AudioClip>(musicOverrideID != null ? musicOverrideID : ID);
            if(instance.playerMusicSource.clip != clip)
            {
                instance.playerMusicSource.clip = clip;
                instance.playerMusicSource.Play();
            }
        }
    }

    public static void StopMusic()
    {
        if(instance.playerMusicSource != null && musicOverrideID == null) // ensure no override
        {
            instance.playerMusicSource.Stop();

            instance.playerMusicSource.clip = null; // clear song
        }
    }
}

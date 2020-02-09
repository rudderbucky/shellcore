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
    
    // change this ID to override sector music
    public static string musicOverrideID;
    void Start() 
    {
        instance = this;
        ChangeMasterVolume(PlayerPrefs.GetFloat("MasterVolume", 0.5F));
        ChangeMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 1));
        ChangeSoundEffectsVolume(PlayerPrefs.GetFloat("SFXVolume", 1));
    }

    public void ChangeMasterVolume(float newVol) 
    {
        masterMixer.SetFloat("MasterVolume", Mathf.Log10(newVol) * 20);
        PlayerPrefs.SetFloat("MasterVolume", newVol);
    }

    public void ChangeMusicVolume(float newVol) 
    {
        masterMixer.SetFloat("MusicVolume", Mathf.Log10(newVol) * 20);
        PlayerPrefs.SetFloat("MusicVolume", newVol);
    }

    public void ChangeSoundEffectsVolume(float newVol) 
    {
        masterMixer.SetFloat("SFXVolume", Mathf.Log10(newVol) * 20);
        PlayerPrefs.SetFloat("SFXVolume", newVol);
    }

    public static void PlayClipByID(string ID, Vector3 pos) {
        var source = new GameObject().AddComponent<AudioSource>();
        source.transform.position = pos;
        source.name = "Audio One-Shot";
        source.outputAudioMixerGroup = instance.sounds;
        source.clip = ResourceManager.GetAsset<AudioClip>(ID);
        source.Play();
        Destroy(source.gameObject, source.clip.length);
    }

    // Plays the clip directly on the player
    public static void PlayClipByID(string ID, bool clear=false) {
        if(ResourceManager.Instance.playerSource != null) {
            if(clear) ResourceManager.Instance.playerSource.Stop();
            if(ID != null) ResourceManager.Instance.playerSource.PlayOneShot(ResourceManager.GetAsset<AudioClip>(ID), 1F);
            // can pass null just to clear the sound buffer
        }
        // TODO: Add audio sources to places that need it
    }

    // Use for OST
    public static void PlayMusic(string ID, bool loop=true)
    {
        if(ResourceManager.Instance.playerMusicSource != null)
        {
            ResourceManager.Instance.playerMusicSource.loop = loop;
            var clip = ResourceManager.GetAsset<AudioClip>(musicOverrideID != null ? musicOverrideID : ID);
            if(ResourceManager.Instance.playerMusicSource.clip != clip)
            {
                ResourceManager.Instance.playerMusicSource.clip = clip;
                ResourceManager.Instance.playerMusicSource.Play();
            }
        }
    }

    public static void StopMusic()
    {
        if(ResourceManager.Instance.playerMusicSource != null && musicOverrideID == null) // ensure no override
        {
            ResourceManager.Instance.playerMusicSource.Stop();

            ResourceManager.Instance.playerMusicSource.clip = null; // clear song
        }
    }
}

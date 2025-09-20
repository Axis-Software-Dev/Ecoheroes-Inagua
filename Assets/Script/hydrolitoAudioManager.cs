using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using System;
public class hydrolitoAudioManager : MonoBehaviour
{
    public Sound[] sounds;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    private void Awake()
    {
        foreach (Sound s in sounds)
        {
            s.source=gameObject.AddComponent<AudioSource>();
            s.source.clip= s.clip;
            s.source.volume=s.volume;
            s.source.pitch= s.pitch;
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void audioPlay(string audioName)
    {
        Sound toPlaySound =Array.Find(sounds, sound => sound.name==audioName);
        toPlaySound.source.Play();
    }

}

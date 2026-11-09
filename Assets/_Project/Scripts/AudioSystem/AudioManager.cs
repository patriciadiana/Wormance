
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource SFXSource;

    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("Sound Effects")]
    [SerializeField] private List<AudioClip> soundEffects = new List<AudioClip>();

    private void Start()
    {
        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    public void PlaySFX(int index)
    {
        Debug.Log("PlaySFX called with index: " + index);

        if (SFXSource == null)
        {
            Debug.LogError("SFX AudioSource is missing!");
            return;
        }

        if (index < 0 || index >= soundEffects.Count)
        {
            Debug.LogError("Invalid sound index! List count: " + soundEffects.Count);
            return;
        }

        if (soundEffects[index] == null)
        {
            Debug.LogError("AudioClip at index " + index + " is missing!");
            return;
        }

        SFXSource.PlayOneShot(soundEffects[index]);

        Debug.Log("Playing sound: " + soundEffects[index].name);
    }

    public void StopSFX()
    {
        SFXSource.Stop();
    }
}

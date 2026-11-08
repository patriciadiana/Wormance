using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider soundSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider SFXSlider;

    private string settingsFilePath;

    private void Start()
    {
        settingsFilePath = Path.Combine(Application.persistentDataPath, "volumeSettings.json");

        if (File.Exists(settingsFilePath))
        {
            LoadVolume();
        }
        else
        {
            SetSoundVolume();
            SetMusicVolume();
            SetSFXVolume();
            SaveVolume();
        }

        soundSlider.onValueChanged.AddListener(delegate { SetSoundVolume(); SaveVolume(); });
        musicSlider.onValueChanged.AddListener(delegate { SetMusicVolume(); SaveVolume(); });
        SFXSlider.onValueChanged.AddListener(delegate { SetSFXVolume(); SaveVolume(); });
    }

    public void SetSoundVolume()
    {
        float volume = soundSlider.value;
        audioMixer.SetFloat("SoundVolume", Mathf.Log10(volume) * 20);
    }
    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
    }
    public void SetSFXVolume()
    {
        float volume = SFXSlider.value;
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
    }
    private void SaveVolume()
    {
        VolumeData data = new VolumeData
        {
            soundVolume = soundSlider.value,
            musicVolume = musicSlider.value,
            sfxVolume = SFXSlider.value
        };

        File.WriteAllText(settingsFilePath, JsonUtility.ToJson(data));
    }
    private void LoadVolume()
    {
        string json = File.ReadAllText(settingsFilePath);
        VolumeData data = JsonUtility.FromJson<VolumeData>(json);

        soundSlider.value = data.soundVolume;
        musicSlider.value = data.musicVolume;
        SFXSlider.value = data.sfxVolume;

        SetSoundVolume();
        SetMusicVolume();
        SetSFXVolume();
    }
}
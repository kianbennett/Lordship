using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager> {

    [System.Serializable]
    public class CustomAudio {

        [SerializeField] private AudioClip clip;
        [SerializeField] private float volume = 1;

        public float Volume { get { return volume; } }

        public void PlayAsMusic() {
            instance.sourceMusic.clip = clip;
            instance.sourceMusic.Play();
            instance.sourceMusic.volume = (OptionsManager.instance.volumeMusic.Value / 10f) * volume;
        }

        public void PlayAsSFX(bool randomPitch = false) {
            float pitch = randomPitch ? Random.Range(0.95f, 1.05f) : 1;
            instance.sourceSFX.pitch = pitch;

            instance.sourceSFX.PlayOneShot(clip, volume);
        }
    }

    public AudioSource sourceMusic;
    public AudioSource sourceSFX;

    [Header("Music")]
    [AudioClip] public CustomAudio musicBackground;

    [Header("SFX")]
    [AudioClip] public CustomAudio sfxButtonClick;

    private bool musicMuted;

    void Update() {
        float musicVolume = musicMuted ? 0 : (OptionsManager.instance.volumeMusic.Value / 10f) * musicBackground.Volume;
        sourceMusic.volume = Mathf.MoveTowards(sourceMusic.volume, musicVolume, Time.deltaTime * 1f);
    }

    public void PlayButtonClick() {
        sfxButtonClick.PlayAsSFX();
    }

    public void FadeOutMusic() {
        musicMuted = true;
    }

    public void ResumeMusic() {
        musicMuted = false;
        sourceMusic.Play();
    }
}

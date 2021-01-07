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
            instance.musicPlaying = this;
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
    [AudioClip] public CustomAudio musicTown;

    [Header("SFX")]
    [AudioClip] public CustomAudio sfxButtonClick;
    [AudioClip] public CustomAudio sfxBlip;

    private bool musicMuted;
    private CustomAudio musicPlaying;

    public float MusicPlayingVolume { get { return musicPlaying != null ? musicPlaying.Volume : 0; } }

    void Update() {
        float musicVolume = musicMuted ? 0 : (OptionsManager.instance.volumeMusic.Value / 10f) * MusicPlayingVolume;
        sourceMusic.volume = Mathf.MoveTowards(sourceMusic.volume, musicVolume, Time.deltaTime * 1f);
    }

    // Put this in its own function so it can be called by buttons
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

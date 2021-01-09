using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using UnityEngine.Rendering.PostProcessing;

public class OptionsManager : Singleton<OptionsManager> {

    public class Option {
        private string key;
        private int value;
        private int defaultValue;

        private Action<int> onValueChange;

        public int Value { get { return value; } }
        public bool BoolValue { get { return value > 0; } }

        public Option(string key, int defaultValue, Action<int> onValueChange) {
            this.key = key;
            value = this.defaultValue = defaultValue;
            this.onValueChange = onValueChange;
            LoadFromPlayerPrefs();
        }

        public void SetValue(int value, bool save) {
            this.value = value;
            onValueChange(value);
            if (save) SaveToPlayerPrefs();
        }

        public void ResetToDefault() {
            SetValue(defaultValue, true);
        }

        public void SaveToPlayerPrefs() {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
        }

        public void LoadFromPlayerPrefs() {
            if(PlayerPrefs.HasKey(key)) SetValue(PlayerPrefs.GetInt(key), false);
        }
    }

    public Option volumeMusic, volumeSFX, highQuality, fullscreen;

    private int screenInitX, screenInitY;

    protected override void Awake() {
        screenInitX = Screen.width;
        screenInitY = Screen.height;

        volumeMusic = new Option("volumeMusic", 8, onChangeVolumeMusic);
        volumeSFX = new Option("volumeSFX", 8, onChangeVolumeSFX);
        highQuality = new Option("highQuality", 1, onChangeHighQuality);
        fullscreen = new Option("fullscreen", 0, onChangeFullscreen);
    }

    private void onChangeVolumeMusic(int value) {
        AudioManager.instance.sourceMusic.volume = value / 10f * AudioManager.instance.MusicPlayingVolume;
    }

    private void onChangeVolumeSFX(int value) {
        AudioManager.instance.sourceSFX.volume = value / 10f;
    }

    private void onChangeHighQuality(int value) {
        // Toggle depth of field, AA and SSAO
        if(CameraController.instance) {
            CameraController.instance.SetPostProcessingEffectEnabled<AmbientOcclusion>(value == 1);
            CameraController.instance.SetPostProcessingEffectEnabled<Bloom>(value == 1);
            CameraController.instance.SetAntialiasingEnabled(value == 1);
        }
    }

    private void onChangeFullscreen(int value) {
        bool fullscreen = value > 0;
        if(fullscreen) {
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
        } else {
            Screen.SetResolution(screenInitX, screenInitY, false);
        }
    }
}

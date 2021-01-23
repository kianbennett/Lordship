using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : MonoBehaviour 
{

    [SerializeField] private Slider musicVolumeSlider, sfxVolumeSlider;
    [SerializeField] private TextMeshProUGUI musicVolumeValue, sfxVolumeValue;
    [SerializeField] private Toggle highQualityToggle, fullscreenToggle;

    private bool playSfx;

    public void SetActive(bool active) 
    {
        gameObject.SetActive(active);

        if(active) {
            // Set playSfx to false before setting values so sound effect doesn't play when the menu is opened (callbacks will be called here)
            playSfx = false;

            musicVolumeSlider.value = OptionsManager.instance.volumeMusic.Value;
            sfxVolumeSlider.value = OptionsManager.instance.volumeSFX.Value;
            highQualityToggle.isOn = OptionsManager.instance.highQuality.BoolValue;
            fullscreenToggle.isOn = OptionsManager.instance.fullscreen.BoolValue;

            playSfx = true;
        }
    }

    public void Return() 
    {
        AudioManager.instance.PlayButtonClick();
        SetActive(false);
    }

    // UI Callbacks

    public void SetMusicVolumeValue() 
    {
        OptionsManager.instance.volumeMusic.SetValue((int) musicVolumeSlider.value, true);
        musicVolumeValue.text = OptionsManager.instance.volumeMusic.Value.ToString();
        if(playSfx) AudioManager.instance.sfxBlip.PlayAsSFX();
    }

    public void SetSFXVolumeValue() 
    {
        OptionsManager.instance.volumeSFX.SetValue((int) sfxVolumeSlider.value, true);
        sfxVolumeValue.text = OptionsManager.instance.volumeSFX.Value.ToString();
        if(playSfx) AudioManager.instance.sfxBlip.PlayAsSFX();
    }

    public void SetHighQualityValue() 
    {
        OptionsManager.instance.highQuality.SetValue(highQualityToggle.isOn ? 1 : 0, true);
        if(playSfx) AudioManager.instance.PlayButtonClick();
    }

    public void SetFullscreenValue() 
    {
        OptionsManager.instance.fullscreen.SetValue(fullscreenToggle.isOn ? 1 : 0, true);
        if(playSfx) AudioManager.instance.PlayButtonClick();
    }
}

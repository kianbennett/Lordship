using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {

    public void SetActive(bool active) {
        gameObject.SetActive(active);
        HUD.instance.darkOverlay.SetActive(active);
        AudioManager.instance.PlayButtonClick();
    }

    public void Resume() {
        AudioManager.instance.PlayButtonClick();
        LevelManager.instance.SetPaused(false);
    }

    public void Options() {
        HUD.instance.optionsMenu.SetActive(true);
        AudioManager.instance.PlayButtonClick();
    }
    
    public void Controls() {
        HUD.instance.tutorialMenu.ShowControls(true);
        AudioManager.instance.PlayButtonClick();
    }

    public void Quit() {
        AudioManager.instance.PlayButtonClick();
        HUD.instance.screenFader.FadeOut(delegate {
            Time.timeScale = 1; // Otherwise will enter main menu while paused
            SceneManager.LoadScene("MainMenu");
        }, true);
    }
}

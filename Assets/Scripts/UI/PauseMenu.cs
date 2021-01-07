using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void Quit() {
        AudioManager.instance.PlayButtonClick();
    }
}

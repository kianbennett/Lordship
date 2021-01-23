using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialMenu : MonoBehaviour 
{
    [SerializeField] private TextMeshProUGUI textTitle;
    [SerializeField] private TextMeshProUGUI textTutorialInfo, textControlsInfo, accessInfo;
    [SerializeField] private Button buttonPlay, buttonControls, buttonContinue;

    private bool fromPauseMenu;

    private void show(bool controls) 
    {
        gameObject.SetActive(true);
        textTitle.text = controls ? "Controls" : "Tutorial";
        textTutorialInfo.gameObject.SetActive(!controls);
        textControlsInfo.gameObject.SetActive(controls);
        buttonPlay.gameObject.SetActive(!controls);
        buttonControls.gameObject.SetActive(!controls);
        buttonContinue.gameObject.SetActive(controls);
    }

    public void ShowTutorial(Season season, int year) 
    {
        show(false);
        textTutorialInfo.text = textTutorialInfo.text.Replace("{season}", season.ToString()).Replace("{year}", year.ToString());
        fromPauseMenu = false;
    }

    public void ShowControls(bool fromPauseMenu) 
    {
        this.fromPauseMenu = fromPauseMenu;
        accessInfo.gameObject.SetActive(!fromPauseMenu);
        show(true);
    }

    public void Play() 
    {
        AudioManager.instance.PlayButtonClick();
        gameObject.SetActive(false);

        if(!fromPauseMenu)
        {
            LevelManager.instance.SetPaused(false, false);
        }
    }

    public void Controls()
    {
        AudioManager.instance.PlayButtonClick();
        ShowControls(false);
    }
}

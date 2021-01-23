using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class EndingMenu : MonoBehaviour 
{
    [SerializeField] private TextMeshProUGUI textTitle;
    [SerializeField] private TextMeshProUGUI textInfoVictory, textInfoDefeat;
    [SerializeField] private Button buttonContinue;

    public void Show(bool victory, int votes) 
    {
        gameObject.SetActive(true);
        textTitle.text = victory ? "Victory!" : "Defeat!";
        textInfoVictory.gameObject.SetActive(victory);
        textInfoDefeat.gameObject.SetActive(!victory);
        if(victory) 
        {
            textInfoVictory.text = textInfoVictory.text.Replace("{votes}", votes.ToString());
            AudioManager.instance.sfxVictory.PlayAsSFX();
        } 
        else 
        {
            AudioManager.instance.sfxDefeat.PlayAsSFX();
        }
    }

    public void Continue() 
    {
        AudioManager.instance.PlayButtonClick();
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
}

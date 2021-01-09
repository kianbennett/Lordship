using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    [Header("Camera")]
    [SerializeField] private Transform cameraContainer;
    [SerializeField] private float cameraRotSpeed, skyRotSpeed;

    [Header("UI")]
    [SerializeField] private Button[] buttons;
    [SerializeField] private ScreenFader screenFader;
    [SerializeField] private OptionsMenu optionsMenu;

    void Awake() {
        TownGenerator.instance.Generate(true);
        TownGenerator.instance.npcSpawner.SpawnNpcs();
    }

    void Start() {
        AudioManager.instance.musicMainMenu.PlayAsMusic();
    }

    void Update() {
        cameraContainer.Rotate(Vector3.up * cameraRotSpeed * Time.deltaTime);
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * skyRotSpeed);
    }

    public void Play() {
        // Set all buttons to non-interactable so they can't be clicked during fade
        foreach(Button button in buttons) button.interactable = false;
        AudioManager.instance.PlayButtonClick();
        AudioManager.instance.FadeOutMusic();

        screenFader.FadeOut(delegate {
            SceneManager.LoadScene("TownScene");
        });
    }

    public void Options() {
        optionsMenu.SetActive(true);
        AudioManager.instance.PlayButtonClick();
    }

    public void Quit() {
        Application.Quit();
        AudioManager.instance.PlayButtonClick();
    }
}
